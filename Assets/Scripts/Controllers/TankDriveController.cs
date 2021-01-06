using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UnityTank
{
    [Serializable]
    public class TankDriveControlInput
    {
        public bool accelerateForward;
        public bool accelerateReverse;
        public bool turnLeft;
        public bool turnRight;
        public void init(GamePlayerState playerState)
        {
            playerState.config.inputActionForward.performed += (InputAction.CallbackContext context) =>
            {
                this.accelerateForward = true;
            };
            playerState.config.inputActionForward.canceled += (InputAction.CallbackContext context) =>
            {
                this.accelerateForward = false;
            };
            playerState.config.inputActionReverse.performed += (InputAction.CallbackContext context) =>
            {
                this.accelerateReverse = true;
            };
            playerState.config.inputActionReverse.canceled += (InputAction.CallbackContext context) =>
            {
                this.accelerateReverse = false;
            };
            playerState.config.inputActionTurnLeft.performed += (InputAction.CallbackContext context) =>
            {
                this.turnLeft = true;
            };
            playerState.config.inputActionTurnLeft.canceled += (InputAction.CallbackContext context) =>
            {
                this.turnLeft = false;
            };
            playerState.config.inputActionTurnRight.performed += (InputAction.CallbackContext context) =>
            {
                this.turnRight = true;
            };
            playerState.config.inputActionTurnRight.canceled += (InputAction.CallbackContext context) =>
            {
                this.turnRight = false;
            };
            this.reset();
        }

        private void InputActionForward_performed(InputAction.CallbackContext obj)
        {
            throw new NotImplementedException();
        }

        public void reset()
        {
            this.accelerateForward = false;
            this.accelerateReverse = false;
            this.turnLeft = false;
            this.turnRight = false;
        }
    }

    [Serializable]
    public class TankDriveController
    {
        public float moveControlResistance = 0.5f;
        public float turnControlResistance = 1.0f;    
        public float moveControlAcceleration = 1.0f;
        public float turnControlAcceleration = 2.0f; 
        public float currentMoveValue = 0.0f;
        public float currentTurnValue = 0.0f;

        public void update(TankDriveControlInput input, float elapsedTime)
        {
            float accelerationFactor = elapsedTime * this.moveControlAcceleration;
            float turningFactor = elapsedTime * this.turnControlAcceleration;
            float accelerateValue = input.accelerateForward? 1.0f : input.accelerateReverse ? -1.0f : 0.0f;
            float turnValue = input.turnRight?1.0f : input.turnLeft ? -1.0f : 0.0f;

            float newMoveValue = Mathf.Clamp(
                this.currentMoveValue + accelerateValue * accelerationFactor, 
                -1.0f, 
                1.0f
            );

            float moveResistanceFactor = elapsedTime * this.moveControlResistance;
            this.currentMoveValue = Mathf.MoveTowards(newMoveValue, 0.0f, moveResistanceFactor);

            float newTurnValue = Mathf.Clamp(
                this.currentTurnValue + turnValue * turningFactor, 
                -1.0f, 
                1.0f
            );

            float turnResistanceFactor = elapsedTime * this.turnControlResistance;
            this.currentTurnValue = Mathf.MoveTowards(newTurnValue, 0.0f, turnResistanceFactor);
        }

        public void reset()
        {
            this.currentMoveValue = 0.0f;
            this.currentTurnValue = 0.0f;
        }
    }
}