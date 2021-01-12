using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using Photon.Realtime;
using Photon.Pun;

namespace UnityTank
{
    [Serializable]
    public class TankController : MonoBehaviourPun, IPunInstantiateMagicCallback
    {
        public float hitPoints = 100.0f;
        public bool controlEnabled = false;

        public TankDriveControlInput driveControlInput = new TankDriveControlInput();
        public TankDriveController driveController = new TankDriveController();
        public TankGunControlInput gunControlInput = new TankGunControlInput();
        public TankGunController gunController = new TankGunController();
        public TankVfxController vfxController = new TankVfxController();
        public TankKinematicController kinematicController = new TankKinematicController();
        public TankSfxController sfxController = new TankSfxController();
        public PlayerGameState playerState = null;

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            GameManager gm = FindObjectOfType<GameManager>();
            gm.RegisterNetworkGameObject(this.gameObject, info);
        }

        private void OnShotTriggered()
        {
            this.gunController.shot();
            this.gunControlInput.enable = false;
            this.gunControlInput.reset();
        }

        private void OnGunReady()
        {
            this.gunControlInput.enable = true;
            this.gunControlInput.reset();
            this.sfxController.playGunReady();
        }

        private void OnGunLoading()
        {
            this.gunControlInput.enable = false;
            this.gunControlInput.reset();
            this.sfxController.playAfterShot();
        }

        private void OnGunCooldown()
        {
            this.gunControlInput.enable = false;
            this.gunControlInput.reset();
        }

        public void init(PlayerGameState state, bool enableControl = true)
        {
            this.controlEnabled = enableControl;
            this.playerState = state;

            if (enableControl)
            {
                this.driveControlInput.init(state);
                this.gunControlInput.init(state);
                state.config.inputActionGameMenu.performed += (InputAction.CallbackContext context) =>
                {
                    GameManager gm = FindObjectOfType<GameManager>();
                    gm.QuitGame();
                };
            }

            this.gunController.stateTrigger -= this.OnShotTriggered;
            this.gunController.stateReady -= this.OnGunReady;
            this.gunController.stateLoading -= this.OnGunLoading;
            this.gunController.stateCooldown -= this.OnGunCooldown;

            this.gunController.stateTrigger += this.OnShotTriggered;
            this.gunController.stateReady += this.OnGunReady;
            this.gunController.stateLoading += this.OnGunLoading;
            this.gunController.stateCooldown += this.OnGunCooldown;

            this.vfxController.setColor(state.config.color);
            this.vfxController.setPlayerLabel(state.config.name);

            this.reset();
        }

        public void reset()
        {
            this.hitPoints = this.playerState.config.startHitPoint;
            this.vfxController.setHealthValue(100.0f);
            this.vfxController.setLoadingIndicator(100.0f);
            this.vfxController.setGunAimingState(0.0f);
            gameObject.transform.position = this.playerState.config.spawnPoint.transform.position;
            gameObject.transform.rotation = this.playerState.config.spawnPoint.transform.rotation;
            this.gunControlInput.enable = true;
            this.gunControlInput.reset();
        }

        public void takeDamage(float hit)
        {
            this.hitPoints -= hit;
            float healthPercent = 100.0f * (this.hitPoints / this.playerState.config.startHitPoint);
            this.vfxController.setHealthValue(healthPercent);
            if (this.hitPoints <= 0.0f)
            {
                this.OnDeath();
            }
        }

        public void OnDeath()
        {
            if (gameObject.activeSelf)
            {
                this.vfxController.showDestroyFx(this.gameObject.transform);
                GameManager gameManager = FindObjectOfType<GameManager>();
                gameManager.SetPlayerState(PlayerState.Disabled);
            }
        }

        private void Awake()
        {
            this.kinematicController.init(GetComponent<Rigidbody>());
            this.vfxController.init(this.gameObject);
            this.sfxController.init(this.gameObject);
        }

        private void OnEnable()
        {
            if (this.controlEnabled)
            {
                this.gunController.reset();
                this.gunControlInput.reset();
                this.driveController.reset();
                this.driveControlInput.reset();
                this.kinematicController.setEnable();
                this.sfxController.setEnable();
            }
        }

        private void OnDisable()
        {
            this.kinematicController.setEnable(false);
            this.sfxController.setEnable(false);
        }

        private void FixedUpdate()
        {
            if (this.controlEnabled)
            {
                this.driveController.update(this.driveControlInput, Time.deltaTime);
                this.gunController.update(this.gunControlInput, Time.deltaTime);
            }
            this.kinematicController.update(
                this.driveController.currentMoveValue,
                this.driveController.currentTurnValue,
                Time.deltaTime
            );
        }

        private void Update()
        {
            this.vfxController.setLoadingIndicator(this.gunController.loadingProgress);
            this.vfxController.setGunAimingState(this.gunController.aimFireForcePercent);
            this.sfxController.update();
        }
    }
}