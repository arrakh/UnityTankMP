using UnityEngine;

namespace UniTank
{
    public class TankShellController : MonoBehaviour
    {
        public bool explodeOnCollision = true;
        public bool explodeOnLifetimeEnd = true;
        protected bool isCollided = false;
        protected TankShell shell = null;
        protected float lifeTime = 3.0f;
        protected Tank shooter;
        public void Init(Tank owner, TankShell shell = null)
        {
            this.shooter = owner;
            this.shell = shell;
            if (this.shell == null)
            {
                this.shell = this.GetComponent<TankShell>();
                this.lifeTime = this.shell.lifeTime;
            }
        }

        public void SetVelocity(Vector3 v)
        {
            Rigidbody body = this.GetComponent<Rigidbody>();
            if (body)
            {
                body.velocity = v;
            }
        }

        public Tank GetShooter()
        {
            return this.shooter;
        }

        public TankShell GetShell()
        {
            return this.shell;
        }

        public void Explode()
        {
            this.GetShooter().GetPlayer().GetGame().arena.OnShellExploded(shell);
            this.DestroyShell();
        }

        protected virtual void DestroyShell()
        {
            this.GetShooter().GetPlayer().GetGame().DestroyObject(this.gameObject);
        }

        protected virtual void Update()
        {
            this.lifeTime -= Time.deltaTime;
            if (this.lifeTime < 0.0f && !isCollided)
            {
                if (this.explodeOnLifetimeEnd)
                {
                    this.Explode();
                }
                else
                {
                    this.DestroyShell();
                }
            }
        }

        protected void OnTriggerEnter(Collider other)
        {
            this.isCollided = true;
            this.GetShooter().GetPlayer().GetGame().arena.OnShellCollided(this.shell, other);
            if (this.explodeOnCollision)
            {
                this.Explode();
            }
            else
            {
                this.DestroyShell();
            }
        }
    }
}