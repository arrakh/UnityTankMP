using System;

namespace UniTank
{
    public abstract class TankPlayer
    {
        public enum State { Init, Ready, Playing, Disabled };
        protected GameManager game;
        protected State state;
        protected Tank tank;

        public TankPlayer(GameManager game)
        {
            this.game = game;
        }

        public GameManager GetGame()
        {
            return this.game;
        }

        public State GetState()
        {
            return this.state;
        }

        public virtual int GetPlayerNumber()
        {
            return this.GetGame().GetPlayerIndex(this);
        }

        protected virtual bool SetState(State state)
        {
            this.state = state;
            if (this.state == State.Disabled)
            {
                this.tank.gameObject.SetActive(false);
            }
            else if (this.state == State.Init)
            {
                this.tank.gameObject.SetActive(true);
            }
            else if (this.state == State.Playing)
            {
                this.tank.gameObject.SetActive(true);
            }
            return this.state == state;
        }

        public virtual void SetControledTank(Tank tank)
        {
            this.tank = tank;
            tank.Init(this);
        }

        public virtual Tank GetTank()
        {
            return this.tank;
        }

        public virtual void PrepareRound(int round)
        {
            if (this.tank)
            {
                this.SetState(State.Init);
                this.tank.Reset();
                this.SetState(State.Ready);
            }
        }

        public virtual void StartRound(int round)
        {
            this.SetState(State.Playing);
        }

        public virtual void EndRound(int round)
        {
            this.SetState(State.Disabled);
        }

        public abstract String GetName();
    }
}