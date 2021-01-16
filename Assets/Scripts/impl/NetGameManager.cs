using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using System.Collections.Generic;

namespace UniTank
{
    public class NetGameManager : GameManager
    {
        public bool isHost = false;
        public GameObject tankPrefab;
        protected Dictionary<string, NetTankPlayer> playerIdMap = new Dictionary<string, NetTankPlayer>();
        public bool NetPlayerJoin(Player netPlayer)
        {
            if(this.GetPlayerCount() < this.config.maxPlayer)
            {
                NetTankPlayer player = new NetTankPlayer(this, netPlayer);
                GameObject tankObject = PhotonNetwork.Instantiate(
                    this.tankPrefab.name,
                    this.arena.gameObject.transform.position,
                    this.arena.gameObject.transform.rotation
                );
                this.playerIdMap.Add(netPlayer.UserId, player);
                tankObject.name = player.GetName() + " Tank";
                Tank tank = tankObject.GetComponent<Tank>();                

                this.AddPlayer(player, tank);
                return true;
            }
            return false;
        }

        public bool NetPlayerLeave(Player netPlayer)
        {
            NetTankPlayer tankPlayer = null;
            if(this.playerIdMap.TryGetValue(netPlayer.UserId, out tankPlayer))
            {
                this.RemovePlayer(tankPlayer);
                this.playerIdMap.Remove(netPlayer.UserId);
                return true;
            }            
            return false;
        }

        public override GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return PhotonNetwork.Instantiate(prefab.name, position, rotation);
        }
    }
}
