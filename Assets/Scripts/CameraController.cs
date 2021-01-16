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
        protected List<Transform> targets = new List<Transform>();
        protected Vector3 currentCameraVelocity = Vector3.zero;
        protected Vector3 initialPos;

        public void UpdateTarget()
        {
            Tank[] tanks = FindObjectsOfType<Tank>(true);

            foreach (Tank tank in tanks)
            {
                if(!this.targets.Contains(tank.transform))
                {
                    this.targets.Add(tank.transform);
                }                
            }
        }

        public void Reset()
        {
            this.targetPos = this.initialPos;
        }

        protected void Awake()
        {
            this.initialPos = this.transform.position;
        }

        public void Start()
        {
            Reset();
        }

        private void Update()
        {
            if (this.targets.Count > 0)
            {
                float tanFov = Mathf.Tan(Mathf.Deg2Rad * GetComponent<Camera>().fieldOfView / 2.0f);
                Vector3 minPos = targets[0].position;
                Vector3 maxPos = targets[0].position;

                foreach (Transform target in this.targets)
                {
                    maxPos = Vector3.Max(maxPos, target.position);
                    minPos = Vector3.Min(minPos, target.position);
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
