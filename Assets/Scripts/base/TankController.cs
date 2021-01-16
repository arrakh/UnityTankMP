using UnityEngine;

namespace UniTank
{
    public abstract class TankController : MonoBehaviour
    {
        protected Tank tank;
        public virtual void Init(Tank tank)
        {
            this.tank = tank;
        }
        public Tank GetTank()
        {
            return this.tank;
        }
        public GameManager GetGame()
        {
            return this.tank.GetPlayer().GetGame();
        }
        public GameArena GetArena()
        {
            return this.GetGame().arena;
        }
        public virtual void Reset()
        {
        }
    }
}