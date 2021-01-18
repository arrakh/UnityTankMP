using System;
using UnityEngine;
using Photon.Pun;

namespace UniTank
{
    class NetTank : Tank, IPunInstantiateMagicCallback
    {
        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            NetGameManager gm = FindObjectOfType<NetGameManager>();
            if (gm)
            {
                gm.NetPlayerJoin(info.Sender, this);
            }
        }

        public void OnDestroy()
        {
            NetGameManager gm = FindObjectOfType<NetGameManager>();
            if (gm)
            {
                gm.NetPlayerLeave((NetTankPlayer)this.GetPlayer());
            }
        }

        public bool IsLocal()
        {
            TankPlayer player = this.GetPlayer();
            if(player is NetTankPlayer)
            {
                NetTankPlayer netPlayer = (NetTankPlayer)player;
                return netPlayer.IsLocal();
            }
            return true;
        }
    }
}
