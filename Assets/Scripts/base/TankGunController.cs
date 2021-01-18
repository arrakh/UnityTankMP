using System;
using UnityEngine;

namespace UniTank
{
    public class TankGunController : TankController
    {
        public enum GunState { Ready, Aiming, Trigger, Cooldown, Loading };

        public Transform aimTransform;
        public float minFireForce = 10.0f;
        public float maxFireForce = 30.0f;
        public float maxAimTime = 3.0f;
        public float aimResistance = 0.1f;
        public float cooldownTime = 0.5f;
        public float loadingTime = 3.0f;

        public Action OnStateReady;
        public Action OnStateAiming;
        public Action<float> OnStateTrigger;
        public Action OnStateCooldown;
        public Action OnStateLoading;
        public Action<float> OnLoadingProgress;
        public Action<float, float> OnAiming;
        public GameObject shellPrefab;

        protected float loadingProgress = 100.0f;
        protected float aimFireForcePercent = 0.0f;
        protected float lastTriggerTime;
        protected float lastShotTime;
        protected float loadingStartTime;
        protected float aimStartTime;
        protected float currentShotForce;
        protected bool aimStarted = false;
        protected bool controlEnabled = false;
        protected GunState gunState = GunState.Ready;

        public override void Init(Tank tank)
        {
            base.Init(tank);
            this.GetGame().OnRoundStarted -= this.EnableControl;
            this.GetGame().OnRoundStarted += this.EnableControl;

            this.GetGame().OnRoundEnded -= this.DisableControl;
            this.GetGame().OnRoundEnded += this.DisableControl;

            TankControlInput input = this.tank.gameObject.GetComponent<TankControlInput>();
            if (input != null)
            {
                input.OnFire += this.OnInputFire;
                input.OnAim += OnInputStartAim;
            }
        }

        public void EnableControl()
        {
            this.controlEnabled = true;
        }

        public void DisableControl()
        {
            this.controlEnabled = false;
        }

        public GunState GetGunState()
        {
            return this.gunState;
        }

        protected void SetLoadingProgress(float progress)
        {
            this.loadingProgress = progress;
            if (this.OnLoadingProgress != null)
            {
                this.OnLoadingProgress(progress);
            }
        }

        protected void SetAimingForce(float aimForce)
        {
            this.currentShotForce = Mathf.Clamp(
                    aimForce,
                    this.minFireForce,
                    this.maxFireForce
            );
            float forceRange = this.maxFireForce - this.minFireForce;
            this.aimFireForcePercent = 100.0f * (this.currentShotForce - this.minFireForce) / forceRange;
            if (this.OnAiming != null)
            {
                this.OnAiming(this.aimFireForcePercent, this.currentShotForce);
            }
        }

        protected void SetGunState(GunState state)
        {
            this.gunState = state;
            if (state == GunState.Ready)
            {
                this.SetLoadingProgress(100.0f);
                if (this.OnStateReady != null) this.OnStateReady();
            }
            else if (state == GunState.Aiming)
            {
                this.aimStartTime = Time.time;
                this.SetAimingForce(this.minFireForce);
                if (this.OnStateAiming != null) this.OnStateAiming();
            }
            else if (state == GunState.Trigger)
            {
                this.lastTriggerTime = Time.time;
                if (this.OnStateTrigger != null) this.OnStateTrigger(this.currentShotForce);
                this.TriggerShot();
            }
            else if (state == GunState.Cooldown)
            {
                if (this.OnStateCooldown != null) this.OnStateCooldown();
            }
            else if (state == GunState.Loading)
            {
                this.SetLoadingProgress(0.0f);
                this.loadingStartTime = Time.time;
                if (this.OnStateLoading != null) this.OnStateLoading();
            }
        }

        protected void OnInputStartAim()
        {
            if(this.controlEnabled)
            {
                if (this.gunState == GunState.Ready)
                {
                    this.currentShotForce = this.minFireForce;
                    this.SetGunState(GunState.Aiming);
                }
                this.aimStarted = true;
            }
        }

        protected void OnInputFire()
        {
            if(this.controlEnabled)
            {
                if (this.gunState == GunState.Aiming)
                {
                    float elapsed = Time.time - this.aimStartTime;
                    float forceRange = this.maxFireForce - this.minFireForce;
                    float forceIncPerSec = forceRange / this.maxAimTime;
                    float forceIncrement = forceIncPerSec * (float)elapsed;
                    SetAimingForce(this.minFireForce + forceIncrement);

                    this.SetGunState(GunState.Trigger);
                }
                this.aimStarted = false;
            }
        }

        public void TriggerShot()
        {
            if (this.gunState == GunState.Trigger || this.gunState == GunState.Aiming)
            {
                GameObject shellInstance = this.GetGame().Instantiate(
                    this.shellPrefab, 
                    aimTransform.position, 
                    aimTransform.rotation
                );
                TankShellController shellController = shellInstance.GetComponent<TankShellController>();
                if (shellController != null)
                {
                    shellController.Init(this.tank);
                    shellController.SetVelocity(this.currentShotForce * aimTransform.forward);
                    shellInstance.SetActive(true);

                    GameObject shotExplosion = this.GetGame().Instantiate(
                        shellController.GetShell().shotExplosionPrefab,
                        aimTransform.position,
                        aimTransform.rotation
                    );
                    shotExplosion.SetActive(true);
                }

                this.lastShotTime = Time.time;

                float elapsedSinceTrigger = Time.time - this.lastTriggerTime;
                if (elapsedSinceTrigger > this.cooldownTime)
                {
                    this.SetGunState(GunState.Cooldown);
                    this.SetGunState(GunState.Loading);
                }
                else
                {
                    this.SetGunState(GunState.Cooldown);
                }
            }
        }

        protected void Update()
        {
            if (this.gunState == GunState.Aiming)
            {
                float forceRange = this.maxFireForce - this.minFireForce;
                float forceIncPerSec = forceRange / this.maxAimTime;
                SetAimingForce(this.currentShotForce + forceIncPerSec * Time.deltaTime);
            }
            else if (this.gunState == GunState.Cooldown)
            {
                float elapsedSinceTrigger = Time.time - this.lastTriggerTime;
                if (elapsedSinceTrigger > this.cooldownTime)
                {
                    this.SetGunState(GunState.Loading);
                }
            }
            else if (this.gunState == GunState.Loading)
            {
                float elapsedLoading = Time.time - this.loadingStartTime;
                this.SetLoadingProgress(100.0f * elapsedLoading / this.loadingTime);
                if (elapsedLoading > this.loadingTime)
                {
                    this.SetGunState(GunState.Ready);
                    if (this.aimStarted)
                    {
                        this.SetGunState(GunState.Aiming);
                    }
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
            this.currentShotForce = this.minFireForce;
            this.lastTriggerTime = 0.0f;
            this.loadingProgress = 100.0f;
            this.aimFireForcePercent = 0.0f;
            this.SetGunState(GunState.Ready);
        }
    }
}