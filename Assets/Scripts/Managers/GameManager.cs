using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

namespace UnityTank
{
    [Serializable]
    public class GamePlayerConfig
    {
        public Transform spawnPoint;
        public string name;
        public Color color;
        public GameObject tankPrefab;
        public float startHitPoint = 100.0f;
        public InputAction inputActionForward;
        public InputAction inputActionReverse;
        public InputAction inputActionTurnLeft;
        public InputAction inputActionTurnRight;
        public InputAction inputActionFire;
        public InputAction inputActionGameMenu;
    }

    [Serializable]
    public class GamePlayerState
    {
        public int playerIndex;
        public TankController controller;
        public GameObject instance;
        public GamePlayerConfig config;
        public int roundScore;
        public GameManager gameManager;
    }


    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        public enum GameState {Init, RoundStarting, RoundEnding, RoundPlaying, GameEnding, Stopped};
        public GameState state = GameState.Stopped;
        public int scoreToWin = 5;

        public CameraController gameCamera;       // Reference to the CameraControl script for control during different phases.
        public Text message;                  // Reference to the overlay Text to display winning text, etc.
        public GamePlayerConfig[] playerConfigs;
        public GamePlayerState[] playerStates;

        private float stateCountDown = 5.0f;
        private int gameRound = 0;
        public LayerMask tankLayerMask;
        public float maxShellDamage = 25.0f;

        private void SpawnAllTanks()
        {
            this.playerStates = new GamePlayerState[this.playerConfigs.Length];
            for(int i=0 ; i<this.playerConfigs.Length ; i++)
            {
                PlayerInput controllerInput = this.playerConfigs[i].spawnPoint.gameObject.GetComponent<PlayerInput>();
                InputActionMap inputMap = controllerInput.actions.FindActionMap("Player" + (i+1));
                this.playerConfigs[i].inputActionForward = inputMap.FindAction("Forward");
                this.playerConfigs[i].inputActionReverse = inputMap.FindAction("Reverse");
                this.playerConfigs[i].inputActionTurnLeft = inputMap.FindAction("TurnLeft");
                this.playerConfigs[i].inputActionTurnRight = inputMap.FindAction("TurnRight");
                this.playerConfigs[i].inputActionFire = inputMap.FindAction("Fire");
                this.playerConfigs[i].inputActionGameMenu = inputMap.FindAction("GameMenu");

                this.playerStates[i] = new GamePlayerState();
                this.playerStates[i].gameManager = this;
                this.playerStates[i].playerIndex = i+1;
                this.playerStates[i].config = this.playerConfigs[i];
                this.playerStates[i].roundScore = 0;
                this.playerStates[i].instance = GameObject.Instantiate(
                    this.playerConfigs[i].tankPrefab,
                    this.playerConfigs[i].spawnPoint.position,
                    this.playerConfigs[i].spawnPoint.rotation
                );
                this.playerStates[i].controller = this.playerStates[i].instance.GetComponent<TankController>();
                this.playerStates[i].controller.init(this.playerStates[i]);              
            }
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
                tankTarget.takeDamage (Mathf.Clamp(explosionDamage, 0.0f, maxShellDamage));
            }
        }

        private void Start()
        {
            GameManager.instance = this;

            this.state = GameState.Init;
            SpawnAllTanks();
            gameCamera.UpdateTarget();
            gameCamera.Reset();

            this.stateCountDown = 5.0f;
            this.gameRound = 1;
            ResetAllTanks ();
            DisableTankControl ();
            this.state = GameState.RoundStarting;
        }

        private void Update()
        {
            if(this.state == GameState.RoundStarting)
            {
                this.stateCountDown -= Time.deltaTime;
                message.text = "ROUND " + this.gameRound+" starting in "+Mathf.Round(this.stateCountDown);
                if(this.stateCountDown <= 0.0f){                                        
                    message.text = string.Empty;
                    EnableTankControl ();
                    this.state = GameState.RoundPlaying;
                }
            }
            else if(this.state == GameState.RoundPlaying)
            {
                GamePlayerState lastTank = null;
                int tankAlive = 0;
                foreach(GamePlayerState tank in this.playerStates){
                    if (tank.instance.activeSelf)
                    {
                        lastTank = tank;
                        tankAlive++;
                    }
                }
                if( tankAlive <= 1)
                {
                    DisableTankControl ();
                    if(tankAlive <= 0)
                    {
                        message.text = "Round "+this.gameRound+" Draw!";
                        this.stateCountDown = 5.0f;
                        this.gameRound++;
                        this.state = GameState.RoundEnding;
                    }
                    else
                    {
                        message.text = lastTank.config.name+" Win Round "+this.gameRound;
                        lastTank.roundScore++;
                        if( lastTank.roundScore < scoreToWin)
                        {
                            this.stateCountDown = 5.0f;
                            this.gameRound++;
                            this.state = GameState.RoundEnding;
                        }     
                        else
                        {
                            message.text = lastTank.config.name+" Win the Game!";
                            this.stateCountDown = 5.0f;
                            this.state = GameState.GameEnding;
                        }  
                    }
                             
                }
            }
            else if(this.state == GameState.RoundEnding)
            {
                this.stateCountDown -= Time.deltaTime;
                if(this.stateCountDown <= 0.0f){ 
                    ResetAllTanks ();
                    this.stateCountDown = 5.0f;
                    gameCamera.Reset();
                    this.state = GameState.RoundStarting;
                }
            }
            else if(this.state == GameState.GameEnding)
            {
                this.stateCountDown -= Time.deltaTime;
                if(this.stateCountDown <= 0.0f){ 
                    this.state = GameState.Stopped;
                    SceneManager.LoadScene (0);
                }
            }
        }

        private void ResetAllTanks()
        {
            foreach (GamePlayerState player in playerStates)
            {
                player.controller.reset();
            }
        }

        private void EnableTankControl()
        {
            foreach (GamePlayerState player in playerStates)
            {
                player.controller.controlEnabled = true;
            }
        }

        private void DisableTankControl()
        {
            foreach (GamePlayerState player in playerStates)
            {
                player.controller.controlEnabled = false;
            }
        }
    }
}