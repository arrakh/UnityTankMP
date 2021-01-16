using UnityEngine;

namespace UniTank
{
    public class TankShell : MonoBehaviour
    {
        public float lifeTime = 3.0f;
        public float explosionRadius = 4.0f;
        public float explosionForce = 25.0f;
        public float maxShellDamage = 50.0f;        
        public GameObject explosionPrefab;
        public GameObject shotExplosionPrefab;
        protected Tank shooter;
        protected Vector3 initialVelocity;
        public void Init(Tank owner, Vector3 initialVelocity)
        {
            this.shooter = owner;
            this.initialVelocity = initialVelocity;
        }

        public Vector3 GetInitialVelocity()
        {
            return this.initialVelocity;
        }

        public Tank GetShooter()
        {
            return this.shooter;
        }
    }
}


