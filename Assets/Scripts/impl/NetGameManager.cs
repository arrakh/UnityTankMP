using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using System.Collections.Generic;
using System;

namespace UniTank
{
    public class NetGameManager : GameManager, IConnectionCallbacks, IMatchmakingCallbacks
    {
        public string gameVersion = "1.0";
        public string playerName = "";
        public GameObject tankPrefab;
        protected Dictionary<string, NetTankPlayer> playerIdMap = new Dictionary<string, NetTankPlayer>();
        protected Dictionary<string, State> clientGameStates = new Dictionary<string, State>();
        protected State hostState;
        public new void Start()
        {
            Connect();
        }

        public bool IsHost()
        {
            return PhotonNetwork.LocalPlayer.IsMasterClient;
        }

        public void Connect()
        {
            if (PhotonNetwork.IsConnected)
            {
                Debug.Log("Launcher conncted to server!, joining random room");
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                Debug.Log("Launcher connecting to server...");
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.GameVersion = this.gameVersion;
            }
        }

        void Awake()
        {
            PhotonNetwork.AddCallbackTarget(this);
            PhotonNetwork.AutomaticallySyncScene = true;
            if (this.playerName == "")
            {
                this.playerName = Environment.MachineName + "/" + Environment.UserName;
            }
            PhotonNetwork.LocalPlayer.NickName = this.playerName;            
        }

        public bool NetPlayerJoin(Player netPlayer, Tank tank = null)
        {
            if(tank == null && netPlayer.IsLocal)
            {
                NetTankPlayer player = new NetTankPlayer(this, netPlayer);
                GameObject tankObject = PhotonNetwork.Instantiate(
                    this.tankPrefab.name,
                    this.arena.gameObject.transform.position,
                    this.arena.gameObject.transform.rotation
                );
                tankObject.name = player.GetName() + " Tank";
                tank = tankObject.GetComponent<Tank>();

                Debug.Log("Adding local player " + player.GetName());
                this.playerIdMap.Add(netPlayer.UserId, player);
                this.clientGameStates[PhotonNetwork.LocalPlayer.UserId] = State.Init;
                this.AddPlayer(player, tank);

            }
            else if(tank != null && !netPlayer.IsLocal)
            {
                NetTankPlayer player = new NetTankPlayer(this, netPlayer);
                GameObject tankObject = tank.gameObject;
                tankObject.name = player.GetName() + " Tank";

                Debug.Log("Adding remote player " + player.GetName());

                this.playerIdMap.Add(netPlayer.UserId, player);
                this.clientGameStates[PhotonNetwork.LocalPlayer.UserId] = State.Init;
                this.AddPlayer(player, tank);

            }
            return true;
        }

        public bool NetPlayerLeave(NetTankPlayer netPlayer)
        {
            if(this.playerIdMap.ContainsKey(netPlayer.GetUserId()))
            {
                if(this.OnTankRemoved != null)
                {
                    this.OnTankRemoved(netPlayer.GetTank());
                }
                this.RemovePlayer(netPlayer);
                this.playerIdMap.Remove(netPlayer.GetUserId());
                this.clientGameStates.Remove(netPlayer.GetUserId());
                return true;
            }
            else
            {
                return false;
            }
        }

        public NetTankPlayer GetPlayerByUserId(string userId)
        {
            NetTankPlayer player = null;
            this.playerIdMap.TryGetValue(userId, out player);
            return player;
        }

        public override GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return PhotonNetwork.Instantiate(prefab.name, position, rotation);
        }

        public override void DestroyObject(GameObject obj)
        {
            PhotonNetwork.Destroy(obj);
        }

        public void OnConnected()
        {
            Debug.Log("Connected");
        }

        public void OnConnectedToMaster()
        {
            Debug.Log("Connected to master, joining random room");
            PhotonNetwork.JoinRandomRoom();
        }

        public void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("Disconnected with reason " + cause.ToString());
        }

        public void OnRegionListReceived(RegionHandler regionHandler)
        {
            Debug.Log(regionHandler.ToString());
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {            
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {            
        }

        public void OnFriendListUpdate(List<FriendInfo> friendList)
        {            
        }

        public void OnCreatedRoom()
        {
            Debug.Log("Room Created");
        }

        public void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.Log("Failed to create room (" + returnCode.ToString() + ") message: " + message);
        }

        public void OnJoinedRoom()
        {            
            base.Start();
            if (this.NetPlayerJoin(PhotonNetwork.LocalPlayer))
            {
                Debug.Log("Player Count " + this.GetPlayerCount().ToString());
            }
        }

        public void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("Join random room failed (" + returnCode.ToString() + ") message: " + message);
            Debug.Log("Creating new room...");
            PhotonNetwork.CreateRoom(null, new RoomOptions
            {
                MaxPlayers = (byte)this.config.maxPlayer,
                PublishUserId = true
            });
        }

        public void OnLeftRoom()
        {
            Debug.Log("Left Room.");
        }

        public void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.Log("Join room failed code " + returnCode.ToString() + " message: " + message);
        }

        protected override void UpdateStateWaitPlayerReady()
        {
            if(this.IsHost())
            {
                bool allPlayerReady = true;
                foreach (TankPlayer player in players)
                {
                    if (player.GetState() != TankPlayer.State.Ready)
                    {
                        allPlayerReady = false;
                    }
                }
                if(allPlayerReady)
                {
                    bool allClientReady = true;
                    foreach(State clientState in this.clientGameStates.Values)
                    {
                        if(clientState != State.WaitPlayersReady)
                        {
                            allClientReady = false;
                        }
                    }
                    
                    if(allClientReady)
                    {
                        this.SetState(State.RoundStarting);
                    }
                }
            }
            else
            {
                base.UpdateStateWaitPlayerReady();
            }
        }

        protected override void SetState(State state)
        {            
            if(this.state != state)
            {
                PhotonView photonView = PhotonView.Get(this);
                if(this.IsHost())
                {
                    this.hostState = state;                    
                    Debug.Log("RPC [OUT] SetState " + state.ToString());
                    photonView.RPC("RPCSetState", RpcTarget.Others, PhotonNetwork.LocalPlayer.UserId, state);
                    this.clientGameStates[PhotonNetwork.LocalPlayer.UserId] = state;
                    base.SetState(state);
                }
                else
                {
                    if (state == State.RoundStarting &&
                        (hostState == State.RoundStarting || hostState == State.RoundPlaying))
                    {
                        //only start game if host, already stated so
                        Debug.Log("RPC [OUT] SetState " + state.ToString());
                        photonView.RPC("RPCSetState", RpcTarget.Others, PhotonNetwork.LocalPlayer.UserId, state);
                        this.clientGameStates[PhotonNetwork.LocalPlayer.UserId] = state;
                        base.SetState(state);
                    }
                    else if(state != State.RoundStarting)
                    {
                        Debug.Log("RPC [OUT] SetState " + state.ToString());
                        photonView.RPC("RPCSetState", RpcTarget.Others, PhotonNetwork.LocalPlayer.UserId, state);
                        this.clientGameStates[PhotonNetwork.LocalPlayer.UserId] = state;
                        base.SetState(state);
                    }
                }
            }
        }

        [PunRPC]
        public void RPCSetState(String userId, State state)
        {
            NetTankPlayer netPlayer = this.GetPlayerByUserId(userId);
            if(netPlayer != null)
            {                
                if(netPlayer.IsMaster())
                {
                    this.hostState = state;
                    if(state == State.RoundEnding || state == State.GameEnding)
                    {
                        //enforced by host
                        this.SetState(state);
                    }
                }
                this.clientGameStates[userId] = state;
                Debug.Log("RPC [INP from "+netPlayer.GetName()+"] SetState " + state.ToString());
            }            
        }
    }
}
