using UnityEngine;
using Photon.Pun;

namespace UniTank
{
    public class NetTankShellController : TankShellController, IPunInstantiateMagicCallback
    {
        protected override void DestroyShell()
        {
            TankShell shell = this.GetComponent<TankShell>();
            Tank shooter = this.GetShooter();
            if (shooter is NetTank)
            {
                NetTank netShooter = (NetTank)shooter;
                if (netShooter.IsLocal())
                {
                    base.DestroyShell();
                }
            }
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            TankShellController shellController = this.GetComponent<TankShellController>();
            if(shellController)
            {
                NetGameManager gm = FindObjectOfType<NetGameManager>();
                NetTankPlayer player = gm.GetPlayerByUserId(info.Sender.UserId);
                shellController.Init(player.GetTank());
            }
        }
    }
}