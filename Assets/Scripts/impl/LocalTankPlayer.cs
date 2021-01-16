namespace UniTank
{
    public class LocalTankPlayer : TankPlayer
    {        
        public LocalTankPlayer(GameManager game) : base(game)
        {

        }

        public override string GetName()
        {
            return "Player" + (this.GetGame().GetPlayerIndex(this)+1).ToString();
        }
    }
}