using UnityEngine;
using System;

namespace UniTank
{
    public class NetControlKeyboardInput : TankControlKeyboardInput
    {
        public override void Init(Tank tank)
        {
            if(tank.GetPlayer() is NetTankPlayer)
            {
                NetTankPlayer netTankPlayer = (NetTankPlayer)tank.GetPlayer();
                if(netTankPlayer.IsLocal())
                {
                    base.Init(tank);
                }
            }
        }
    }
}