using UnityEngine;
using System;

namespace UniTank
{
    public class GameArena : MonoBehaviour
    {        
        public Action<Tank, GameObject, float> OnShotTrigger;
        public Action<TankShell> OnShellShot;
        public Action<TankShell, Collider> OnShellCollided;
        public Action<TankShell> OnShellExploded;
        public LayerMask tankLayerMask;
        protected GameManager game;
        protected CameraController gameCamera;

        public virtual void Init(GameManager game)
        {
            this.game = game;
            this.game.OnTankSpawn -= this.processNewTank;
            this.game.OnTankSpawn += this.processNewTank;
            this.gameCamera = this.gameObject.GetComponentInChildren<CameraController>();
        }

        public virtual void Reset()
        {
            this.gameCamera.Reset();
        }

        public void Awake()
        {
            this.OnShotTrigger -= this.TriggerShot;
            this.OnShotTrigger += this.TriggerShot;

            this.OnShellShot -= this.ShowShotExplosion;
            this.OnShellShot += this.ShowShotExplosion;

            this.OnShellExploded -= this.ProcessShellExplosion;
            this.OnShellExploded += this.ProcessShellExplosion;

            this.OnShellCollided -= this.ProcessShellCollision;
            this.OnShellCollided += this.ProcessShellCollision;
        }

        protected void processNewTank(Tank tank)
        {
            this.gameCamera.UpdateTarget();
        }

        public Transform GetTankSpawnPoint(Tank tank)
        {
            GameObject[] points = GameObject.FindGameObjectsWithTag("Respawn");
            return points[(this.game.GetPlayerIndex(tank.GetPlayer()) + this.game.GetCurrentRound()) % points.Length].transform;
        }

        public Transform GetPlayerTankSpawnPoint(TankPlayer player)
        {
            GameObject[] points = GameObject.FindGameObjectsWithTag("Respawn");
            return points[(this.game.GetPlayerIndex(player) + this.game.GetCurrentRound()) % points.Length].transform;
        }

        protected void TriggerShot(Tank owner, GameObject shellPrefab, float force)
        {
            Transform aimTransform = owner.GetGunAimTransform();
            if (aimTransform == null)
            {
                aimTransform = owner.transform;
            }
            GameObject shellInstance = GameObject.Instantiate(shellPrefab, aimTransform.position, aimTransform.rotation);
            TankShell shell = shellInstance.GetComponent<TankShell>();
            if (shell != null)
            {
                shell.Init(owner, force * aimTransform.forward);
                shellInstance.SetActive(true);
            }

            if (this.OnShellShot != null)
            {
                this.OnShellShot(shellInstance.GetComponent<TankShell>());
            }
        }

        protected void ShowShotExplosion(TankShell shell)
        {
            Transform explosionTransform = shell.GetShooter().GetGunAimTransform();
            if (explosionTransform == null)
            {
                explosionTransform = shell.gameObject.transform;
            }
            GameObject shotExplosion = GameObject.Instantiate(
                shell.shotExplosionPrefab,
                explosionTransform.position,
                explosionTransform.rotation
            );
            shotExplosion.SetActive(true);
        }

        protected void ProcessShellExplosion(TankShell shell)
        {
            GameObject explosion = Instantiate(shell.explosionPrefab, shell.transform.position, shell.transform.rotation);
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

            if (this.OnShellExploded != null)
            {
                this.OnShellExploded(shell);
            }

            shell.gameObject.SetActive(false);
            Destroy(shell.gameObject);
        }
    }
}
