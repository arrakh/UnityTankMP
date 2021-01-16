using Photon.Realtime;
using Photon.Pun;

namespace UniTank
{
    public class NetTankPlayer : TankPlayer
    {
        protected Player netPlayer;
        public NetTankPlayer(NetGameManager game, Player player) : base(game)
        {
            this.netPlayer = player;
        }

        public override string GetName()
        {
            return this.netPlayer.NickName;
        }

        public override void SetControledTank(Tank tank)
        {
            this.tank = tank;
            tank.Init(this, netPlayer.IsLocal);
        }
    }
}