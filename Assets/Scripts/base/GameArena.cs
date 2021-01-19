using UnityEngine;
using System;

namespace UniTank
{
    public class GameArena : MonoBehaviour
    {
        public Action<TankShell, Collider> OnShellCollided;
        public Action<TankShell> OnShellExploded;
        public LayerMask tankLayerMask;
        protected GameManager game;
        protected CameraController gameCamera;

        public virtual void Init(GameManager game)
        {
            this.game = game;
            this.gameCamera = this.gameObject.GetComponentInChildren<CameraController>();
        }

        public virtual void Reset()
        {
            this.gameCamera.Reset();
        }

        public void Awake()
        {
            this.OnShellExploded -= this.ProcessShellExplosion;
            this.OnShellExploded += this.ProcessShellExplosion;

            this.OnShellCollided -= this.ProcessShellCollision;
            this.OnShellCollided += this.ProcessShellCollision;
        }

        public Transform GetTankSpawnPoint(Tank tank)
        {
            GameObject[] points = GameObject.FindGameObjectsWithTag("Respawn");
            if (points.Length > 0)
            {
                int index = tank.GetPlayer().GetPlayerNumber() + this.game.GetCurrentRound();
                return points[index % points.Length].transform;
            }
            else
            {
                return this.gameObject.transform;
            }
        }

        public Transform GetPlayerTankSpawnPoint(TankPlayer player)
        {
            GameObject[] points = GameObject.FindGameObjectsWithTag("Respawn");
            if (points.Length > 0)
            {
                int index = player.GetPlayerNumber() + this.game.GetCurrentRound();
                return points[index % points.Length].transform;
            }
            else
            {
                return this.gameObject.transform;
            }
        }

        protected void ProcessShellExplosion(TankShell shell)
        {
            GameObject explosion = this.game.Instantiate(shell.explosionPrefab, shell.transform.position, shell.transform.rotation);
            explosion.SetActive(true);

            Collider[] colliders = Physics.OverlapSphere(shell.transform.position, shell.explosionRadius, this.tankLayerMask);
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
        }

        protected void ProcessShellCollision(TankShell shell, Collider collider)
        {
            if (collider)
            {
                Rigidbody targetRigidbody = collider.GetComponent<Rigidbody>();
                Rigidbody shellBody = shell.GetComponent<Rigidbody>();
                if (targetRigidbody)
                {
                    targetRigidbody.AddExplosionForce(
                        shellBody.velocity.magnitude,
                        shell.transform.position,
                        shell.explosionRadius
                    );
                }
            }
        }
    }
}
