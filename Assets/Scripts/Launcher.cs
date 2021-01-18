using System;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

namespace UniTank
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        public byte maxPlayersPerRoom = 2;
        public string gameVersion = "1.0";
        public string playerName = "";
        public NetGameManager gameManager = null;

        void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            if (this.playerName == "")
            {
                this.playerName = Environment.MachineName + "/" + Environment.UserName;
            }
            PhotonNetwork.LocalPlayer.NickName = this.playerName;
            if (gameManager == null)
            {
                this.gameManager = FindObjectOfType<NetGameManager>();
            }
        }

        void Start()
        {
            Connect();
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

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("Join random room failed (" + returnCode.ToString() + ") message: " + message);
            Debug.Log("Creating new room...");
            PhotonNetwork.CreateRoom(null, new RoomOptions
            {
                MaxPlayers = this.maxPlayersPerRoom,
                PublishUserId = true
            });
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.Log("Failed to create room (" + returnCode.ToString() + ") message: " + message);
        }

        public override void OnJoinedRoom()
        {
            if (this.gameManager.NetPlayerJoin(PhotonNetwork.LocalPlayer))
            {
                Debug.Log("Player Count " + this.gameManager.GetPlayerCount().ToString());
            }
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to master, joining random room");
            PhotonNetwork.JoinRandomRoom();
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.Log("Disconnected with reason " + cause.ToString());
        }

        public override void OnLeftRoom()
        {
            Debug.Log("Left Room.");
        }
    }

}
