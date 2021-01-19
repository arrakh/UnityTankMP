using UnityEngine;

namespace UniTank
{
    public class TankDriveController : TankController
    {
        public float moveControlResistance = 0.5f;
        public float turnControlResistance = 1.0f;
        public float moveControlAcceleration = 1.0f;
        public float turnControlAcceleration = 2.0f;
        public float currentMoveValue = 0.0f;
        public float currentTurnValue = 0.0f;
        protected TankControlInput input = null;
        protected bool driveEnabled = false;

        public override void Init(Tank tank)
        {
            base.Init(tank);
            this.GetGame().OnRoundStarted -= this.EnableDrive;
            this.GetGame().OnRoundStarted += this.EnableDrive;

            this.GetGame().OnRoundEnded -= this.DisableDrive;
            this.GetGame().OnRoundEnded += this.DisableDrive;

            this.input = this.tank.gameObject.GetComponent<TankControlInput>();
        }

        public void EnableDrive()
        {
            this.driveEnabled = true;
        }

        public void DisableDrive()
        {
            this.driveEnabled = false;
        }

        protected void FixedUpdate()
        {
            if (this.driveEnabled)
            {
                float accelerationFactor = Time.fixedDeltaTime * this.moveControlAcceleration;
                float turningFactor = Time.fixedDeltaTime * this.turnControlAcceleration;
                float accelerateValue = input.accelerateForward ? 1.0f : input.accelerateReverse ? -1.0f : 0.0f;
                float turnValue = input.turnRight ? 1.0f : input.turnLeft ? -1.0f : 0.0f;

                float newMoveValue = Mathf.Clamp(
                    this.currentMoveValue + accelerateValue * accelerationFactor,
                    -1.0f,
                    1.0f
                );

                float moveResistanceFactor = Time.fixedDeltaTime * this.moveControlResistance;
                this.currentMoveValue = Mathf.MoveTowards(newMoveValue, 0.0f, moveResistanceFactor);

                float newTurnValue = Mathf.Clamp(
                    this.currentTurnValue + turnValue * turningFactor,
                    -1.0f,
                    1.0f
                );

                float turnResistanceFactor = Time.fixedDeltaTime * this.turnControlResistance;
                this.currentTurnValue = Mathf.MoveTowards(newTurnValue, 0.0f, turnResistanceFactor);
            }
        }
    }
}