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
    }
}


