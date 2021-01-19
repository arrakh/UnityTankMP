using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

namespace UniTank
{
    public class GameManager : MonoBehaviour
    {
        public enum State
        {
            Init,
            WaitPlayersJoin,
            WaitPlayersReady,
            RoundStarting,
            RoundEnding,
            RoundPlaying,
            GameEnding,
            Stopped,
            Quit
        };

        [Serializable]
        public class Config
        {
            public int scoreToWIn = 5;
            public int maxRound = -1;
            public int maxPlayer = 2;
            public float roundStartCountdown = 5.0f;
            public float startHitPoint = 100.0f;
        }

        public Config config = new Config();
        public Action<Tank> OnTankDeath;
        public Action<Tank> OnTankSpawn;
        public Action<Tank> OnTankRemoved;
        public Action OnGameStart;
        public Action OnRoundStarting;
        public Action OnRoundStarted;
        public Action OnRoundEnded;
        public Action OnGameEnd;
        public GameArena arena;
        public Text message;
        public Color[] tankColors;

        protected List<TankPlayer> players = new List<TankPlayer>();
        protected Dictionary<TankPlayer, int> playerScores = new Dictionary<TankPlayer, int>();
        protected State state;
        protected int gameRound = 0;
        protected float stateCountDown = 5.0f;

        public void InitGame()
        {
            this.OnTankDeath -= this.ProcessTankDeath;
            this.OnTankDeath += this.ProcessTankDeath;
            if (this.arena == null)
            {
                this.arena = FindObjectOfType<GameArena>(true);
            }
            if (this.arena != null)
            {
                this.arena.Init(this);
                this.arena.gameObject.SetActive(true);
            }
        }

        public void AddPlayer(TankPlayer player, Tank tank, int score = 0)
        {
            this.players.Add(player);
            this.playerScores.Add(player, score);

            Transform tankSpawnPoint = this.arena.GetPlayerTankSpawnPoint(player);
            tank.gameObject.transform.position = tankSpawnPoint.position;
            tank.gameObject.transform.rotation = tankSpawnPoint.rotation;
            tank.color = this.tankColors[this.players.Count % this.tankColors.Length];

            player.SetControledTank(tank.GetComponent<Tank>());

            if (this.OnTankSpawn != null)
            {
                this.OnTankSpawn(tank);
            }
        }

        public void RemovePlayer(TankPlayer player)
        {
            this.players.Remove(player);
            GameObject.Destroy(player.GetTank());
        }

        public void StartGame()
        {
            this.message.text = "Initializing...";
            this.SetState(State.Init);
            this.InitGame();
            if (this.OnGameStart != null)
            {
                this.OnGameStart();
            }
            this.SetState(State.WaitPlayersJoin);
        }

        public void Start()
        {
            this.StartGame();
        }

        protected virtual void UpdateStateWaitPlayerJoin()
        {
            if (this.players.Count >= this.config.maxPlayer)
            {
                this.StarGameRound(1);
            }
        }

        protected virtual void UpdateStateWaitPlayerReady()
        {
            bool allPlayerReady = true;
            foreach (TankPlayer player in players)
            {
                if (player.GetState() != TankPlayer.State.Ready)
                {
                    allPlayerReady = false;
                }
            }
            if (allPlayerReady)
            {
                this.SetState(State.RoundStarting);
            }
        }

        protected void UpdateStateRoundStarting()
        {
            this.stateCountDown -= Time.deltaTime;
            message.text = "ROUND " + this.gameRound + " starting in " + Mathf.Round(this.stateCountDown);
            if (this.stateCountDown <= 0.0f)
            {
                message.text = string.Empty;
                this.SetState(State.RoundPlaying);
            }
        }

        protected void UpdateStateRoundPlaying()
        {
            TankPlayer lastPlayer = null;
            int tankAlive = 0;
            foreach (TankPlayer player in players)
            {
                if (player.GetState() == TankPlayer.State.Playing)
                {
                    tankAlive++;
                    lastPlayer = player;
                }
            }

            if (tankAlive <= 1)
            {
                if (tankAlive <= 0)
                {
                    message.text = "Round " + this.GetCurrentRound().ToString() + " Draw!";
                    this.SetState(State.RoundEnding);
                }
                else
                {
                    message.text = lastPlayer.GetName() + " Win Round " + this.GetCurrentRound();
                    this.SetScore(lastPlayer, this.GetPlayerScore(lastPlayer) + 1);
                    if (this.GetPlayerScore(lastPlayer) < this.config.scoreToWIn)
                    {
                        this.SetState(State.RoundEnding);
                    }
                    else
                    {
                        message.text = lastPlayer.GetName() + " Win the Game!";
                        this.SetState(State.GameEnding);
                    }
                }

            }
        }

        protected void UpdateStateRoundEnding()
        {
            this.stateCountDown -= Time.deltaTime;
            if (this.stateCountDown <= 0.0f)
            {
                this.StarGameRound(this.GetCurrentRound() + 1);
            }
        }

        protected void UpdateStateGameEnding()
        {
            this.stateCountDown -= Time.deltaTime;
            if (this.stateCountDown <= 0.0f)
            {
                this.SetState(State.Stopped);
                SceneManager.LoadScene(0);
            }
        }

        protected void Update()
        {
            if (this.state == State.WaitPlayersJoin)
            {
                this.UpdateStateWaitPlayerJoin();
            }
            else if (this.state == State.WaitPlayersReady)
            {
                this.UpdateStateWaitPlayerReady();
            }
            if (this.state == State.RoundStarting)
            {
                this.UpdateStateRoundStarting();
            }
            else if (this.state == State.RoundPlaying)
            {
                this.UpdateStateRoundPlaying();
            }
            else if (this.state == State.RoundEnding)
            {
                this.UpdateStateRoundEnding();
            }
            else if (this.state == State.GameEnding)
            {
                this.UpdateStateGameEnding();
            }
        }

        protected virtual void SetState(State state)
        {
            if (this.state != state)
            {
                Debug.Log("Game State = " + state.ToString());
                this.state = state;
                if (state == State.WaitPlayersJoin)
                {
                    this.message.text = "Waiting for player to join";
                }
                else if (state == State.WaitPlayersReady)
                {
                    this.message.text = "Waiting for player to ready up";
                }
                else if (state == State.RoundStarting)
                {
                    if (this.OnRoundStarting != null)
                    {
                        this.OnRoundStarting();
                    }
                    this.stateCountDown = 5.0f;
                }
                else if (state == State.RoundPlaying)
                {
                    foreach (TankPlayer player in players)
                    {
                        player.StartRound(this.GetCurrentRound());
                    }
                    if (this.OnRoundStarted != null)
                    {
                        this.OnRoundStarted();
                    }
                }
                else if (state == State.RoundEnding)
                {
                    foreach (TankPlayer player in players)
                    {
                        player.EndRound(this.GetCurrentRound());
                    }
                    this.stateCountDown = 5.0f;
                    if (this.OnRoundEnded != null)
                    {
                        this.OnRoundEnded();
                    }
                }
                else if (state == State.GameEnding)
                {
                    foreach (TankPlayer player in players)
                    {
                        player.EndRound(this.GetCurrentRound());
                    }
                    this.stateCountDown = 5.0f;
                    if (this.OnGameEnd != null)
                    {
                        this.OnGameEnd();
                    }
                }
            }
        }

        protected void StarGameRound(int round)
        {
            this.gameRound = round;
            this.arena.Reset();
            foreach (TankPlayer player in players)
            {
                player.PrepareRound(round);
            }
            this.SetState(State.WaitPlayersReady);
        }

        public State GetState()
        {
            return state;
        }

        public int GetCurrentRound()
        {
            return this.gameRound;
        }

        public int GetPlayerCount()
        {
            return this.players.Count;
        }

        public int GetPlayerIndex(TankPlayer player)
        {
            return this.players.IndexOf(player);
        }

        public int GetPlayerScore(TankPlayer player)
        {
            int score = -1;
            this.playerScores.TryGetValue(player, out score);
            return score;
        }

        public bool SetScore(TankPlayer player, int score)
        {
            if (this.playerScores.ContainsKey(player))
            {
                this.playerScores[player] = score;
                return true;
            }
            return false;
        }

        public TankPlayer GetPlayerByIndex(int index)
        {
            if (index < this.players.Count)
            {
                return this.players[index];
            }
            else
            {
                return null;
            }
        }

        protected void ProcessTankDeath(Tank tank)
        {
            tank.Explode();
            tank.GetPlayer().EndRound(this.GetCurrentRound());
        }

        public virtual GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return GameObject.Instantiate(prefab, position, rotation);
        }

        public virtual void DestroyObject(GameObject obj)
        {
            Destroy(obj);
        }
    }
}
