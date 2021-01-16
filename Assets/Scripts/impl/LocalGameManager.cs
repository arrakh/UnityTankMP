using UnityEngine;
namespace UniTank
{
    public class LocalGameManager : GameManager
    {
        public GameObject tankPrefab;
        public void InitPlayers()
        {
            while (this.players.Count < this.config.maxPlayer)
            {
                LocalTankPlayer player = new LocalTankPlayer(this);
                GameObject tankObject = Instantiate(
                    this.tankPrefab,
                    this.arena.gameObject.transform
                );
                tankObject.name = player.GetName() + "Tank";
                Tank tank = tankObject.GetComponent<Tank>();

                this.AddPlayer(player, tank);
            }
        }

        protected override void UpdateStateWaitPlayerJoin()
        {
            this.InitPlayers();
            base.UpdateStateWaitPlayerJoin();
        }
    }
}