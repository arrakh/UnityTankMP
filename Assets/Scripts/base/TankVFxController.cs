using UnityEngine;
using UnityEngine.UI;

namespace UniTank
{
    public class TankVFxController : TankController
    {
        public Slider uiHealthSlider;
        public Slider uiFireSlider;
        public Slider uiLoadingSlider;
        public Image uiLoadingSliderImage;
        public Text uiPlayerLabel;
        public GameObject tankExplosionPrefab;
        public override void Init(Tank tank)
        {
            base.Init(tank);
            this.uiPlayerLabel.text = this.tank.GetPlayer().GetName();
            this.uiHealthSlider.value = 100.0f * this.tank.GetCurrentHitPoint() / this.GetGame().config.startHitPoint;
            this.SetColor(this.tank.color);

            TankGunController gunController = this.tank.gameObject.GetComponent<TankGunController>();
            if (gunController != null)
            {
                gunController.OnLoadingProgress -= this.OnGunLoading;
                gunController.OnAiming -= this.OnGunAiming;

                gunController.OnLoadingProgress += this.OnGunLoading;
                gunController.OnAiming += this.OnGunAiming;

                gunController.OnStateReady -= this.OnGunReady;
                gunController.OnStateReady += this.OnGunReady;

                gunController.OnStateCooldown -= this.OnGunCooldown;
                gunController.OnStateCooldown += this.OnGunCooldown;
            }

            Tank tankObject = this.tank.GetComponent<Tank>();
            if (tankObject != null)
            {
                tankObject.OnExploded -= OnTankExploded;
                tankObject.OnHitPointChanged -= OnHitPointChanged;

                tankObject.OnExploded += OnTankExploded;
                tankObject.OnHitPointChanged += OnHitPointChanged;
            }

            this.uiFireSlider.value = 0.0f;
        }

        protected void OnGunReady()
        {
            this.uiFireSlider.value = 0.0f;
        }

        protected void OnGunCooldown()
        {
            this.uiFireSlider.value = 0.0f;
        }

        protected void OnGunAiming(float percent, float force)
        {
            this.uiFireSlider.value = percent;
        }

        protected void OnGunLoading(float progress)
        {
            this.uiLoadingSlider.value = progress;
        }

        protected void OnTankExploded()
        {
            this.ShowDestroyFx();
        }

        protected void OnHitPointChanged(float current, float start)
        {
            this.uiHealthSlider.value = 100.0f * current / start;
        }

        public void SetColor(Color color)
        {
            Color colorOffset = new Color(0.2f, 0.2f, 0.2f);
            this.uiPlayerLabel.color = color + colorOffset;
            this.uiLoadingSliderImage.color = color + colorOffset;
            MeshRenderer[] renderers = this.tank.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in renderers)
            {
                renderer.material.color = color;
            }
        }

        public void ShowDestroyFx(Transform tankTransform = null)
        {
            if (tankTransform == null)
            {
                tankTransform = this.tank.gameObject.transform;
            }
            this.GetGame().Instantiate(this.tankExplosionPrefab, tankTransform.position, tankTransform.rotation);
        }
    }
}