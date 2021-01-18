using UnityEngine;
using System.Collections.Generic;

namespace UniTank
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public float moveDelay = 0.5f;
        public float lookSpeed = 1.5f;
        public float viewMargin = 10.0f;
        protected Vector3 targetPos = Vector3.zero;
        protected Vector3 currentCameraVelocity = Vector3.zero;
        protected Vector3 initialPos;
        protected GameManager game;


        public void Reset()
        {
            this.targetPos = this.initialPos;
        }

        protected void Awake()
        {
            this.initialPos = this.transform.position;
            this.game = GameObject.FindObjectOfType<GameManager>();
        }

        public void Start()
        {
            Reset();
        }

        private void Update()
        {            
            if (this.game.GetPlayerCount() > 0)
            {
                GameObject firstTarget = this.game.GetPlayerByIndex(0).GetTank().gameObject;
                float tanFov = Mathf.Tan(Mathf.Deg2Rad * GetComponent<Camera>().fieldOfView / 2.0f);
                Vector3 minPos = firstTarget.transform.position;
                Vector3 maxPos = firstTarget.transform.position;

                for(int i=0; i<this.game.GetPlayerCount(); i++)             
                {
                    GameObject playerTank = this.game.GetPlayerByIndex(i).GetTank().gameObject;
                    maxPos = Vector3.Max(maxPos, playerTank.transform.position);
                    minPos = Vector3.Min(minPos, playerTank.transform.position);
                }
                maxPos += Vector3.one * viewMargin;
                minPos -= Vector3.one * viewMargin;

                Vector3 spread = (maxPos - minPos);
                Vector3 targetLook = minPos + spread * 0.5f;
                float cameraDistance = (0.5f * spread.magnitude / GetComponent<Camera>().aspect) / tanFov;
                Vector3 cameraDirection = (targetLook - initialPos).normalized;

                this.targetPos = targetLook - cameraDirection * cameraDistance;
                this.transform.RotateAround(targetLook, Vector3.up, Time.deltaTime * lookSpeed);

                this.targetPos = (transform.position - targetLook).normalized * cameraDistance + targetLook;

                this.transform.position = Vector3.SmoothDamp(
                    this.transform.position,
                    this.targetPos,
                    ref this.currentCameraVelocity,
                    this.moveDelay
                );

                this.transform.rotation = Quaternion.Slerp(
                    this.transform.rotation,
                    Quaternion.LookRotation(targetLook - this.transform.position),
                    Time.deltaTime * this.lookSpeed
                );
            }
        }
    }
}
