using UnityEngine;

namespace UniTank
{
    public class TankShellController : MonoBehaviour
    {
        public bool explodeOnCollision = true;
        public bool explodeOnLifetimeEnd = true;
        protected bool isCollided = false;

        private void Start()
        {
            Rigidbody body = this.GetComponent<Rigidbody>();
            TankShell shell = this.GetComponent<TankShell>();

            if (body && shell)
            {
                body.velocity = shell.GetInitialVelocity();
            }

            if (shell != null)
            {
                Destroy(this.gameObject, shell.lifeTime);
            }
            else
            {
                Destroy(this.gameObject, 3.0f);
            }
        }

        private void OnDestroy()
        {
            if (!isCollided && this.explodeOnLifetimeEnd)
            {
                TankShell shell = this.gameObject.GetComponent<TankShell>();
                if (shell != null)
                {
                    shell.GetShooter().GetPlayer().GetGame().arena.OnShellExploded(shell);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            this.isCollided = true;
            TankShell shell = this.gameObject.GetComponent<TankShell>();
            if (shell != null)
            {
                shell.GetShooter().GetPlayer().GetGame().arena.OnShellCollided(shell, other);
            }
            if (this.explodeOnCollision)
            {
                shell.GetShooter().GetPlayer().GetGame().arena.OnShellExploded(shell);
            }
        }
    }
}