using System;
using UnityEngine;

namespace UnityTank
{
    [Serializable]
    public class TankSfxController
    {
        public bool muted = false;
        public AudioSource audioEngineIdling;
        public AudioSource audioEngineRunning;
        public AudioSource audioGun;
        public AudioClip clipAfterShot;
        public AudioClip clipGunLoaded;
        private Vector3 lastPosition;
        private float idlingTargetVolume;
        private float runnningTargetVolume;
        private float runningTargetPitch;
        private GameObject tank = null;
        public void init(GameObject tank)
        {
            this.tank = tank;
            lastPosition = tank.transform.position;
        }
        public void setEnable(bool enable = true)
        {
            this.muted = !enable;
            if(this.muted)
            {
                this.audioEngineIdling.volume = 0.0f;
                this.audioEngineRunning.volume = 0.0f;
            }
        }

        public void update()
        {
            if(!this.muted)
            {
                Vector3 movement = this.tank.transform.position - this.lastPosition;
                lastPosition = this.tank.transform.position;

                idlingTargetVolume   = Mathf.Clamp(2.0f*(1.0f - movement.magnitude),0.25f,0.1f);              
                runnningTargetVolume = Mathf.Clamp(movement.magnitude*4.0f,0.0f,1.0f);  
                runningTargetPitch   = Mathf.Clamp(0.5f+movement.magnitude*2.0f, 0.5f,2.0f);

                audioEngineIdling.volume = audioEngineIdling.volume + (idlingTargetVolume - audioEngineIdling.volume)*Time.fixedDeltaTime;
                audioEngineRunning.volume = audioEngineRunning.volume + (runnningTargetVolume - audioEngineRunning.volume)*Time.fixedDeltaTime;
                audioEngineRunning.pitch = audioEngineRunning.pitch + (runningTargetPitch - audioEngineRunning.pitch)*Time.fixedDeltaTime;
            }
        }

        public void playAfterShot()
        {
            this.audioGun.PlayOneShot(this.clipAfterShot);
        }

        public void playGunReady()
        {
            this.audioGun.PlayOneShot(this.clipGunLoaded);
        }
    }
}