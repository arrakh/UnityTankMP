using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Realtime;
using Photon.Pun;

namespace UnityTank
{
    [Serializable]
    public class TankGunControlInput
    {
        public bool aimingFire;
        public bool fireTrigger;
        public bool enable = true;
        public void init(PlayerGameState playerState)
        {
            playerState.config.inputActionFire.performed += (InputAction.CallbackContext context) =>
            {
                if (this.enable)
                {
                    this.aimingFire = true;
                    this.fireTrigger = false;
                }

            };
            playerState.config.inputActionFire.canceled += (InputAction.CallbackContext context) =>
            {
                if (this.enable)
                {
                    this.fireTrigger = true;
                    this.aimingFire = false;
                }
            };
            this.reset();
        }
        public void reset()
        {
            this.aimingFire = false;
            this.fireTrigger = false;
        }
    }

    [Serializable]
    public class TankGunController
    {
        public enum GunState { Ready, Trigger, Cooldown, Loading };
        public GunState gunState = GunState.Ready;
        public Transform aimTransform;
        public float currentShotForce;
        public float lastTriggerTime;
        public float lastShotTime;
        public float startLoadingTime;
        public float minFireForce = 10.0f;
        public float maxFireForce = 30.0f;
        public float maxAimTime = 3.0f;
        public float aimFireForcePercent = 0.0f;
        public float aimResistance = 0.1f;
        public float cooldownTime = 0.5f;
        public float loadingTime = 3.0f;
        public float loadingProgress = 100.0f;
        public Action stateReady;
        public Action stateTrigger;
        public Action stateCooldown;
        public Action stateLoading;
        public GameObject shellPrefab;
        public GameObject shootExplosionPrefab;

        private void setState(GunState state)
        {
            this.gunState = state;
            if (state == GunState.Ready)
            {
                if (this.stateReady != null) this.stateReady();
            }
            else if (state == GunState.Trigger)
            {
                if (this.stateTrigger != null) this.stateTrigger();
            }
            else if (state == GunState.Cooldown)
            {
                if (this.stateCooldown != null) this.stateCooldown();
            }
            else if (state == GunState.Loading)
            {
                if (this.stateLoading != null) this.stateLoading();
            }
        }

        public TankGunController()
        {
            this.reset();
        }

        public GameObject shot()
        {
            this.lastShotTime = Time.time;
            PhotonNetwork.Instantiate(
                this.shootExplosionPrefab.name,
                this.aimTransform.position,
                this.aimTransform.rotation
            );

            GameObject shellInstance = PhotonNetwork.Instantiate(
                this.shellPrefab.name,
                this.aimTransform.position,
                this.aimTransform.rotation
            );

            shellInstance.GetComponent<Rigidbody>().velocity = this.currentShotForce * this.aimTransform.forward;

            this.currentShotForce = this.minFireForce;
            float elapsedSinceTrigger = Time.time - this.lastTriggerTime;
            if (elapsedSinceTrigger > this.cooldownTime)
            {
                this.startLoadingTime = Time.time + this.cooldownTime;
                this.setState(GunState.Cooldown);
                this.setState(GunState.Loading);
            }
            else
            {
                this.setState(GunState.Cooldown);
            }

            return shellInstance;
        }

        public void update(TankGunControlInput input, float elapsedTime)
        {
            if (this.gunState == GunState.Ready)
            {
                if (input.fireTrigger)
                {
                    this.lastTriggerTime = Time.time;
                    this.setState(GunState.Trigger);
                }
                else
                {
                    float newForceValue = 0.0f;
                    float forceRange = this.maxFireForce - this.minFireForce;
                    if (input.aimingFire)
                    {
                        float forceIncPerSec = forceRange / this.maxAimTime;
                        float forceIncrement = forceIncPerSec * elapsedTime;
                        newForceValue = Mathf.Clamp(
                            this.currentShotForce + forceIncrement,
                            this.minFireForce,
                            this.maxFireForce
                        );
                    }

                    float aimForceReduced = elapsedTime * aimResistance;
                    this.currentShotForce = Mathf.Clamp(
                        Mathf.MoveTowards(newForceValue, this.minFireForce, aimForceReduced),
                        this.minFireForce,
                        this.maxFireForce);
                    this.aimFireForcePercent = 100.0f * (this.currentShotForce - this.minFireForce) / forceRange;
                }
            }
            else if (this.gunState == GunState.Cooldown)
            {
                float elapsedSinceTrigger = Time.time - this.lastTriggerTime;
                if (elapsedSinceTrigger > this.cooldownTime)
                {
                    this.startLoadingTime = this.lastShotTime + this.cooldownTime;
                    this.loadingProgress = 0.0f;
                    this.setState(GunState.Loading);
                }
            }
            else if (this.gunState == GunState.Loading)
            {
                float elapsedLoading = Time.time - this.startLoadingTime;
                this.loadingProgress = 100.0f * elapsedLoading / this.loadingTime;
                if (elapsedLoading > this.loadingTime)
                {
                    this.setState(GunState.Ready);
                }
            }
        }

        public void reset()
        {
            this.currentShotForce = this.minFireForce;
            this.lastTriggerTime = 0.0f;
            this.loadingProgress = 100.0f;
            this.aimFireForcePercent = 0.0f;
            this.gunState = GunState.Ready;
        }
    }
}