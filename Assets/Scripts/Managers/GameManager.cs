using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using Photon.Realtime;
using Photon.Pun;

namespace UnityTank
{
    public enum GameState { Init, GameWaiting, RoundStarting, RoundEnding, RoundPlaying, GameEnding, Stopped };
    public enum PlayerState { Waiting, Ready, Playing, Disabled }

    [Serializable]
    public class GamePlayerConfig
    {
        public Transform spawnPoint = null;
        public string name;
        public Color color;
        public GameObject tankPrefab = null;
        public float startHitPoint = 100.0f;
        public float maxShellDamage = 25.0f;
        public InputAction inputActionForward;
        public InputAction inputActionReverse;
        public InputAction inputActionTurnLeft;
        public InputAction inputActionTurnRight;
        public InputAction inputActionFire;
        public InputAction inputActionGameMenu;
    }

    [Serializable]
    public class PlayerGameState
    {        
        public PlayerGameState(Player player, GamePlayerConfig config)
        {
            this.player = player;
            this.config = config;
        }
        public Player player;
        public GamePlayerConfig config;        
        public PlayerState playerState = PlayerState.Disabled;
        public GameState gameState = GameState.Init;
        public TankController controller = null;
        public GameObject instance = null;
        public int roundScore = 0;
    }

    public class GameManager : MonoBehaviourPun, IPunPrefabPool
    {
        public List<GameObject> networkObjectPrefabs = new List<GameObject>();

        private GameState state = GameState.Stopped;
        public int scoreToWin = 5;        
        public bool isHost = false;
        public LayerMask tankLayerMask;

        public CameraController gameCamera = null; 
        public Text message;        
        public GameObject defaultTankPrefab;
        private Dictionary<String, PlayerGameState> playerIdMap = new Dictionary<string, PlayerGameState>();
        private PlayerGameState localPlayerState = null;               

        private float stateCountDown = 5.0f;
        private int gameRound = 0;

        public bool JoinPlayer(Player player, GamePlayerConfig config)
        {
            if(this.state == GameState.Stopped)
            {
                if (!this.playerIdMap.ContainsKey(player.UserId))
                {
                    PlayerGameState playerState = new PlayerGameState(player, config);
                    this.playerIdMap.Add(player.UserId, playerState);
                    if(playerState.config.tankPrefab == null)
                    {
                        playerState.config.tankPrefab = this.defaultTankPrefab;
                    }
                    if(player.IsLocal)
                    {
                        this.localPlayerState = playerState;
                    }
                    return true;
                }
            }
            return false;
        }

        public bool LeavePlayer(Player player)
        {
            if (this.state == GameState.Stopped)
            {
                if (this.playerIdMap.ContainsKey(player.UserId))
                {
                    this.playerIdMap.Remove(player.UserId);
                }
            }
            return false;
        }

        public int CountPlayers()
        {
            return this.playerIdMap.Count;
        }

        public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
        {
            foreach (GameObject item in this.networkObjectPrefabs)
            {
                if (item.name == prefabId)                {                    GameObject go = UnityEngine.Object.Instantiate(item, position, rotation);                    if (go.tag == "Player")                    {
                        CameraController camCtrl = FindObjectOfType<CameraController>();                        if (camCtrl)                        {                            camCtrl.UpdateTarget(go);                        }                    }                    go.SetActive(false);                    return go;                }
            }
            return null;
        }

        public void Destroy(GameObject gameObject)
        {
        }

        private void InitPlayers()
        {
            GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("Respawn");
            this.localPlayerState.playerState = PlayerState.Waiting;
            this.localPlayerState.config.spawnPoint = spawnPoints[this.localPlayerState.player.ActorNumber % spawnPoints.Length].transform;
            this.localPlayerState.instance = PhotonNetwork.Instantiate(
                this.localPlayerState.config.tankPrefab.name,
                this.localPlayerState.config.spawnPoint.position,
                this.localPlayerState.config.spawnPoint.rotation
            );
            this.localPlayerState.instance.name = "LocalTank";
            PlayerInput controllerInput = GetComponent<PlayerInput>();
            InputActionMap inputMap = controllerInput.actions.FindActionMap("Player1");
            this.localPlayerState.config.inputActionForward = inputMap.FindAction("Forward");
            this.localPlayerState.config.inputActionReverse = inputMap.FindAction("Reverse");
            this.localPlayerState.config.inputActionTurnLeft = inputMap.FindAction("TurnLeft");
            this.localPlayerState.config.inputActionTurnRight = inputMap.FindAction("TurnRight");
            this.localPlayerState.config.inputActionFire = inputMap.FindAction("Fire");
            this.localPlayerState.config.inputActionGameMenu = inputMap.FindAction("GameMenu");


            this.localPlayerState.controller = this.localPlayerState.instance.GetComponent<TankController>();
            this.localPlayerState.controller.init(this.localPlayerState, true);
            this.SetPlayerState(PlayerState.Ready);
        }

        public void OnShellExploded(ShellController shell)
        {
            GameObject explosion = Instantiate(shell.explosionPrefab, shell.transform.position, shell.transform.rotation);
            Collider[] colliders = Physics.OverlapSphere (shell.transform.position, shell.explosionRadius, tankLayerMask);
            foreach(Collider collider in colliders)
            {
                Rigidbody targetRigidbody = collider.GetComponent<Rigidbody> ();
                if(targetRigidbody)
                {        
                    IncurDamage(targetRigidbody, shell);
                }
            }

            Destroy(shell.gameObject);
        }

        public void IncurDamage(Rigidbody body, ShellController shell)
        {
            Rigidbody shellBody = shell.GetComponent<Rigidbody>();
            float explosionForce = shell.explosionForce*shellBody.velocity.magnitude;
            body.AddExplosionForce( explosionForce, shell.transform.position, shell.explosionRadius);

            TankController tankTarget = body.GetComponent<TankController> ();
            if(tankTarget)
            {
                float explosionDistance = (tankTarget.transform.position - shell.transform.position).magnitude;
                float relativeDistance = (shell.explosionRadius - explosionDistance) / shell.explosionRadius;
                float explosionDamage = explosionForce * relativeDistance;
                tankTarget.takeDamage (Mathf.Clamp(explosionDamage, 0.0f, tankTarget.playerState.config.maxShellDamage));
            }
        }

        private void StartGame()
        {
            if(this.localPlayerState != null)
            {
                if (this.gameCamera == null)
                {
                    this.gameCamera = (CameraController)GameObject.FindObjectOfType(typeof(CameraController));
                }
                this.SetGameState(GameState.Init);
                InitPlayers();
                gameCamera.Reset();

                this.stateCountDown = 5.0f;
                this.gameRound = 1;

                this.localPlayerState.controller.controlEnabled = false;
                this.localPlayerState.controller.reset();

                this.SetGameState(GameState.GameWaiting);
            }
        }

        [PunRPC]
        private void RPCSetPlayerState(string userId, PlayerState netState)
        {
            PlayerGameState state;            if (this.playerIdMap.TryGetValue(userId, out state))            {                state.playerState = netState;                Debug.Log("[RPC-inp] set game state from " + userId + " to " + netState.ToString());            }
        }

        public void SetPlayerState(PlayerState netState)
        {
            this.localPlayerState.playerState = netState;
            this.photonView.RPC("RPCSetPlayerState", RpcTarget.Others, this.localPlayerState.player.UserId, netState);
            Debug.Log("[RPC-out] set player state " + netState.ToString());
        }

        [PunRPC]
        private void RPCSetGameState(string userId, GameState state)
        {
            Debug.Log("[RPC-inp] set game state from " + userId + " to " + state.ToString());
            PlayerGameState playerGameState;
            if (this.playerIdMap.TryGetValue(userId, out playerGameState))
            {
                playerGameState.gameState = state;
            }
        }

        private void SetGameState(GameState state)
        {
            this.state = state;
            this.localPlayerState.gameState = state;
            this.photonView.RPC("RPCSetGameState", RpcTarget.Others, this.localPlayerState.player.UserId, state);
            Debug.Log("[RPC] set game state " + state.ToString());
        }

        private bool IsGameStateInSync()
        {
            if (this.state != GameState.RoundStarting && this.state != GameState.RoundEnding)            {                foreach (KeyValuePair<String, PlayerGameState> player in this.playerIdMap)                {                    if (player.Value.gameState != this.state)                    {                        if (this.isHost)                        {                            this.SetGameState(this.state);                        }                        return false;                    }                }            }            return true;
        }

        private void Awake()
        {
            PhotonNetwork.PrefabPool = this;
            DontDestroyOnLoad(this.gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("Scene Loaded : " + scene.name);
            if(scene.name == "Game")
            {
                this.StartGame();
            }
        }

        private void Update()
        {
            if (this.state == GameState.GameWaiting)
            {
                message.text = "ROUND " + this.gameRound.ToString() + " waiting for other players ";
                bool allPlayersReady = true;
                foreach (KeyValuePair<String, PlayerGameState> player in this.playerIdMap)
                {
                    if (player.Value.playerState == PlayerState.Waiting)
                    {
                        allPlayersReady = false;
                    }
                }
                if (allPlayersReady)
                {
                    this.SetGameState(GameState.RoundStarting);
                }
            }
            else if(this.state == GameState.RoundStarting)
            {
                this.stateCountDown -= Time.deltaTime;
                message.text = "ROUND " + this.gameRound.ToString() +" starting in "+Mathf.Round(this.stateCountDown).ToString();
                if(this.stateCountDown <= 0.0f){                                        
                    message.text = string.Empty;
                    this.localPlayerState.controller.controlEnabled = true;
                    this.SetGameState(GameState.RoundPlaying);
                    this.SetPlayerState(PlayerState.Playing);
                }
            }
            else if(this.state == GameState.RoundPlaying)
            {
                PlayerGameState lastTank = null;
                int tankAlive = 0;
                foreach(KeyValuePair<String, PlayerGameState> player in this.playerIdMap){
                    if (player.Value.playerState != PlayerState.Disabled)
                    {
                        lastTank = player.Value;
                        tankAlive++;
                    }
                }
                if( tankAlive <= 1)
                {
                    this.localPlayerState.controller.controlEnabled = false;
                    if (tankAlive <= 0)
                    {
                        message.text = "Round "+this.gameRound+" Draw!";
                        this.stateCountDown = 5.0f;
                        this.gameRound++;
                        this.SetGameState(GameState.RoundEnding);
                    }
                    else
                    {
                        message.text = lastTank.config.name+" Win Round "+this.gameRound;
                        lastTank.roundScore++;
                        if( lastTank.roundScore < scoreToWin)
                        {
                            this.stateCountDown = 5.0f;
                            this.gameRound++;
                            this.SetGameState(GameState.RoundEnding);
                        }     
                        else
                        {
                            message.text = lastTank.config.name+" Win the Game!";
                            this.stateCountDown = 5.0f;
                            this.SetGameState(GameState.GameEnding);
                        }  
                    }
                             
                }
            }
            else if(this.state == GameState.RoundEnding)
            {
                this.SetPlayerState(PlayerState.Disabled);
                this.stateCountDown -= Time.deltaTime;
                if(this.stateCountDown <= 0.0f){
                    this.SetPlayerState(PlayerState.Waiting);
                    this.localPlayerState.controller.reset();                    
                    this.stateCountDown = 5.0f;
                    gameCamera.Reset();
                    this.SetPlayerState(PlayerState.Ready);
                    this.SetGameState(GameState.GameWaiting);
                }
            }
            else if(this.state == GameState.GameEnding)
            {
                this.stateCountDown -= Time.deltaTime;
                if(this.stateCountDown <= 0.0f){ 
                    this.SetGameState(GameState.Stopped);
                    PhotonNetwork.LoadLevel("Launcher");
                }
            }
        }
    }
}