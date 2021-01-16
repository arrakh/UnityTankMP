using UnityEngine;
using System;

namespace UniTank
{
    public class Tank : MonoBehaviour
    {
        protected TankPlayer player = null;
        protected bool localTank = true;
        public Color color;
        public Action OnExploded;
        public Action<float, float> OnHitPointChanged;

        public bool IsLocal()
        {
            return this.localTank;
        }
        public TankPlayer GetPlayer()
        {
            return this.player;
        }

        public float GetStartHitPoint()
        {
            return this.player.GetGame().config.startHitPoint;
        }

        protected float currentHitPoint = 100.0f;
        public float GetCurrentHitPoint()
        {
            return currentHitPoint;
        }

        public virtual void Init(TankPlayer owner, bool isLocal = true)
        {
            this.localTank = isLocal;
            this.player = owner;
            this.currentHitPoint = this.GetStartHitPoint();
            this.player.GetGame().arena.OnShellExploded -= this.ResponseToExplosion;
            this.player.GetGame().arena.OnShellExploded += this.ResponseToExplosion;
            TankController[] controllers = this.gameObject.GetComponents<TankController>();
            foreach (TankController controller in controllers)
            {
                controller.Init(this);
            }
        }

        public virtual void Reset()
        {
            this.SetHitPoint(this.GetStartHitPoint());
            TankController[] controllers = this.gameObject.GetComponents<TankController>();
            foreach(TankController controller in controllers)
            {
                controller.Reset();
            }
        }

        public virtual void TakeDamage(float hpDamage)
        {
            this.SetHitPoint(this.GetCurrentHitPoint() - hpDamage);
            if (this.GetCurrentHitPoint() <= 0.0f)
            {
                this.GetPlayer().GetGame().OnTankDeath(this);
            }
        }

        public void Explode()
        {
            if(this.OnExploded != null)
            {
                this.OnExploded();
            }
        }

        public virtual void SetHitPoint(float hp)
        {
            this.currentHitPoint = Mathf.Clamp(hp, 0.0f, this.GetStartHitPoint());
            if(this.OnHitPointChanged != null)
            {
                this.OnHitPointChanged(this.currentHitPoint, this.GetStartHitPoint());
            }
        }

        public Transform GetGunAimTransform()
        {
            TankGunController gunController = this.GetComponent<TankGunController>();
            if(gunController)
            {
                return gunController.aimTransform;
            }
            return null;
        }

        protected void ResponseToExplosion(TankShell shell)
        {
            float explosionDistance = (this.transform.position - shell.transform.position).magnitude;
            if (explosionDistance < shell.explosionRadius)
            {
                Rigidbody shellBody = shell.GetComponent<Rigidbody>();
                float relativeDistance = (shell.explosionRadius - explosionDistance) / shell.explosionRadius;
                float explosionForce = shell.explosionForce * shellBody.velocity.magnitude;
                float explosionDamage = explosionForce * relativeDistance;
                this.TakeDamage(Mathf.Clamp(explosionDamage, 0.0f, shell.maxShellDamage));
            }
        }
    }
}