using System;
using UnityEngine;

namespace UnityTank
{
    [Serializable]
    public class TankKinematicController
    {
        public float moveSpeed = 10.0f;
        public float turnSpeed = 100.0f;
        private Rigidbody body;
        public void init(Rigidbody body)
        {
            this.body = body;
        }
        public void setEnable(bool enable = true)
        {
            this.body.isKinematic = false;
        }

        public void update(float move, float turn, float elapsedTime)
        {
            Transform transform = body.gameObject.transform;
            float moveFactor = Mathf.MoveTowards(move, 0.0f, 0.1f * Mathf.Abs(turn));
            Vector3 movement = transform.forward * moveFactor * this.moveSpeed * elapsedTime;
            this.body.MovePosition(transform.position + movement);

            float turnMovement = turn * this.turnSpeed * Time.deltaTime;
            float turnFactor = Mathf.MoveTowards(turnMovement, 0.0f, 0.1f * Mathf.Abs(move));
            Quaternion turnRotation = Quaternion.Euler (0f, turnFactor, 0f);
            this.body.MoveRotation(transform.rotation * turnRotation);
        }
    }
}