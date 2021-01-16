using UnityEngine.InputSystem;
using UnityEngine;

namespace UniTank
{
    public class TankControlKeyboardInput : TankControlInput
    {
        public InputAction inputActionForward;
        public InputAction inputActionReverse;
        public InputAction inputActionTurnLeft;
        public InputAction inputActionTurnRight;
        public InputAction inputActionFire;
        public InputAction inputActionGameMenu;

        public override void Init(Tank tank)
        {
            base.Init(tank);
            PlayerInput playerInput = FindObjectOfType<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.actionEvents[0].AddListener(this.performInputForward);
                
                InputActionMap inputMap = playerInput.actions.FindActionMap("TankControl");
                if (inputMap != null)
                {
                    this.inputActionForward = inputMap.FindAction(this.GetTank().GetPlayer().GetName() + "/Forward");
                    if(this.inputActionForward == null)
                    {
                        this.inputActionForward = inputMap.FindAction("Forward");
                    }

                    this.inputActionReverse = inputMap.FindAction(this.GetTank().GetPlayer().GetName() + "/Reverse");
                    if (this.inputActionReverse == null)
                    {
                        this.inputActionReverse = inputMap.FindAction("Reverse");
                    }

                    this.inputActionTurnLeft = inputMap.FindAction(this.GetTank().GetPlayer().GetName() + "/TurnLeft");
                    if (this.inputActionTurnLeft == null)
                    {
                        this.inputActionTurnLeft = inputMap.FindAction("TurnLeft");
                    }

                    this.inputActionTurnRight = inputMap.FindAction(this.GetTank().GetPlayer().GetName() + "/TurnRight");
                    if (this.inputActionTurnRight == null)
                    {
                        this.inputActionTurnRight = inputMap.FindAction("TurnRight");
                    }

                    this.inputActionFire = inputMap.FindAction(this.GetTank().GetPlayer().GetName() + "/Fire");
                    if (this.inputActionFire == null)
                    {
                        this.inputActionFire = inputMap.FindAction("Fire");
                    }

                    this.inputActionGameMenu = inputMap.FindAction(this.GetTank().GetPlayer().GetName() + "/GameMenu");
                    if (this.inputActionGameMenu == null)
                    {
                        this.inputActionGameMenu = inputMap.FindAction("GameMenu");
                    }

                    this.inputActionForward.performed -= this.performInputForward;
                    this.inputActionForward.performed += this.performInputForward;
                    this.inputActionForward.canceled -= this.cancelInputForward;
                    this.inputActionForward.canceled += this.cancelInputForward;

                    this.inputActionReverse.performed -= this.performInputReverse;
                    this.inputActionReverse.performed += this.performInputReverse;
                    this.inputActionReverse.canceled -= this.cancelInputReverse;
                    this.inputActionReverse.canceled += this.cancelInputReverse;

                    this.inputActionTurnLeft.performed -= this.performInputTurnLeft;
                    this.inputActionTurnLeft.performed += this.performInputTurnLeft;
                    this.inputActionTurnLeft.canceled -= this.cancelInputTurnLeft;
                    this.inputActionTurnLeft.canceled += this.cancelInputTurnLeft;

                    this.inputActionTurnRight.performed -= this.performInputTurnRight;
                    this.inputActionTurnRight.performed += this.performInputTurnRight;
                    this.inputActionTurnRight.canceled -= this.cancelInputTurnRight;
                    this.inputActionTurnRight.canceled += this.cancelInputTurnRight;

                    this.inputActionFire.performed -= this.performInputFire;
                    this.inputActionFire.performed += this.performInputFire;
                    this.inputActionFire.canceled -= this.cancelInputFire;
                    this.inputActionFire.canceled += this.cancelInputFire;

                    this.inputActionGameMenu.performed -= this.performInputGameMenu;
                    this.inputActionGameMenu.performed += this.performInputGameMenu;
                    this.inputActionGameMenu.canceled -= this.cancelInputGameMenu;
                    this.inputActionGameMenu.canceled += this.cancelInputGameMenu;
                }
                else
                {
                    Debug.LogError("could not find input action map for TankControl");
                }
            }
        }

        public void performInputForward(InputAction.CallbackContext context)
        {
            this.accelerateForward = true;
        }

        public void cancelInputForward(InputAction.CallbackContext context)
        {
            this.accelerateForward = false;
        }

        public void performInputReverse(InputAction.CallbackContext context)
        {
            this.accelerateReverse = true;
        }

        public void cancelInputReverse(InputAction.CallbackContext context)
        {
            this.accelerateReverse = false;
        }

        public void performInputTurnLeft(InputAction.CallbackContext context)
        {
            this.turnLeft = true;
        }

        public void cancelInputTurnLeft(InputAction.CallbackContext context)
        {
            this.turnLeft = false;
        }

        public void performInputTurnRight(InputAction.CallbackContext context)
        {
            this.turnRight = true;
        }

        public void cancelInputTurnRight(InputAction.CallbackContext context)
        {
            this.turnRight = false;
        }

        public void performInputFire(InputAction.CallbackContext context)
        {
            this.fire = true;
            if (this.OnAim != null)
            {
                this.OnAim();
            }
        }

        public void cancelInputFire(InputAction.CallbackContext context)
        {
            if (this.OnFire != null)
            {
                this.OnFire();
            }
            this.fire = false;
        }

        public void performInputGameMenu(InputAction.CallbackContext context)
        {
            this.gameMenu = true;
        }

        public void cancelInputGameMenu(InputAction.CallbackContext context)
        {
            if (this.OnGameMenu != null)
            {
                this.OnGameMenu();
            }

            this.gameMenu = false;
        }
    }
}
