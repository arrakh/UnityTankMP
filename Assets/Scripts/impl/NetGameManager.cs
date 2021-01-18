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
                this.AddPlayer(player, tank);

            }
            else if(tank != null && !netPlayer.IsLocal)
            {
                NetTankPlayer player = new NetTankPlayer(this, netPlayer);
                GameObject tankObject = tank.gameObject;
                tankObject.name = player.GetName() + " Tank";

                Debug.Log("Adding remote player " + player.GetName());

                this.playerIdMap.Add(netPlayer.UserId, player);
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
    }
}
