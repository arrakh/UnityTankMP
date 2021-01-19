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

        public bool IsLocal()
        {            
            return this.netPlayer.IsLocal;
        }

        public bool IsMaster()
        {
            return this.netPlayer.IsMasterClient;
        }

        public string GetUserId()
        {
            return this.netPlayer.UserId;
        }

        public override int GetPlayerNumber()
        {
            return netPlayer.ActorNumber;
        }
    }
}