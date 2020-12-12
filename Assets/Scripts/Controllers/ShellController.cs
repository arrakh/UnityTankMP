using UnityEngine;

namespace UnityTank
{
    public class ShellController : MonoBehaviour
    {
        public float lifeTime = 3.0f;
        public float explosionRadius = 3.0f;
        public float explosionForce = 10.0f;
        public GameObject explosionPrefab;
        private bool collided = false;
        private void Start ()
        {
            Destroy (gameObject, lifeTime);
        }

        private void OnDestroy()
        {
            if(!collided)
            {
                GameManager[] gm = FindObjectsOfType<GameManager>();
                if(gm.Length > 0)
                {
                    gm[0].OnShellExploded(this);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            this.collided = true;
            GameManager[] gm = FindObjectsOfType<GameManager>();
            if(gm.Length > 0)
            {
                gm[0].OnShellExploded(this);
            }
        }
    }
}