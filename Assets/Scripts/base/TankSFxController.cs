using UnityEngine;

namespace UniTank
{
    public class TankSFxController : TankController
    {
        public bool muted = false;
        public AudioSource audioEngineIdling;
        public AudioSource audioEngineRunning;
        public AudioSource audioGun;
        public AudioClip clipGunLoaded;

        private Vector3 lastPosition = Vector3.zero;
        private float idlingTargetVolume;
        private float runnningTargetVolume;
        private float runningTargetPitch;
        public override void Init(Tank tank)
        {
            base.Init(tank);
            lastPosition = this.tank.transform.position;
            TankGunController gunController = this.gameObject.GetComponent<TankGunController>();
            if (gunController)
            {
                gunController.OnStateReady -= this.playGunReady;
                gunController.OnStateReady += this.playGunReady;
            }
        }

        protected void FixedUpdate()
        {
            Vector3 movement = this.tank.transform.position - this.lastPosition;
            lastPosition = this.tank.transform.position;

            idlingTargetVolume = Mathf.Clamp(2.0f * (1.0f - movement.magnitude), 0.25f, 0.1f);
            runnningTargetVolume = Mathf.Clamp(movement.magnitude * 4.0f, 0.0f, 1.0f);
            runningTargetPitch = Mathf.Clamp(0.5f + movement.magnitude * 2.0f, 0.5f, 2.0f);

            this.audioEngineIdling.volume = audioEngineIdling.volume + (idlingTargetVolume - audioEngineIdling.volume) * Time.fixedDeltaTime;
            this.audioEngineRunning.volume = audioEngineRunning.volume + (runnningTargetVolume - audioEngineRunning.volume) * Time.fixedDeltaTime;
            this.audioEngineRunning.pitch = audioEngineRunning.pitch + (runningTargetPitch - audioEngineRunning.pitch) * Time.fixedDeltaTime;

            if (this.muted)
            {
                this.audioEngineIdling.volume = 0.0f;
                this.audioEngineRunning.volume = 0.0f;
            }
        }

        public void playGunReady()
        {
            this.audioGun.PlayOneShot(this.clipGunLoaded);
        }
    }
}