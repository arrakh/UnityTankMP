using System;
using UnityEngine;

namespace UnityTank
{
    public class ArenaController : MonoBehaviour
    {
        public LayerMask explosionReceiverLayerMask;
        public Action<ShellController> OnShellExploded;
        public void OnShellCollided(ShellController shell, Collider other)
        {
            GameObject explosion = Instantiate(shell.explosionPrefab, shell.transform.position, shell.transform.rotation);
            Collider[] colliders = Physics.OverlapSphere(shell.transform.position, shell.explosionRadius, explosionReceiverLayerMask);
            Rigidbody shellBody = shell.GetComponent<Rigidbody>();
            float explosionForce = shell.explosionForce * shellBody.velocity.magnitude;
            foreach (Collider collider in colliders)
            {
                Rigidbody targetRigidbody = collider.GetComponent<Rigidbody>();
                if (targetRigidbody)
                {
                    targetRigidbody.AddExplosionForce(explosionForce, shell.transform.position, shell.explosionRadius);
                }
            }

            if (this.OnShellExploded != null)
            {
                this.OnShellExploded(shell);
            }

            Destroy(shell.gameObject);
        }
    }
}