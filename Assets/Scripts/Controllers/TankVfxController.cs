using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityTank
{
    [Serializable]
    public class TankVfxController
    {
        public Slider uiHealthSlider;
        public Slider uiFireSlider;
        public Slider uiLoadingSlider;
        public Image uiLoadingSliderImage;
        public Text uiPlayerLabel;
        public Color baseColor;
        public Color uiBaseColor;
        public GameObject tankExplosionPrefab;
        private GameObject tank;

        public void init(GameObject tank)
        {
            this.tank = tank;
        }

        public void setColor(Color tankColor)
        {
            this.baseColor = tankColor;
            this.uiBaseColor = tankColor + new Color(0.2f,0.2f,0.2f);
            this.uiPlayerLabel.color = this.uiBaseColor;
            this.uiLoadingSliderImage.color = this.uiBaseColor;
            MeshRenderer[] renderers = tank.GetComponentsInChildren<MeshRenderer> ();
            foreach(MeshRenderer renderer in renderers)
            {
                renderer.material.color = tankColor;
            }
        }

        public void setPlayerLabel(string text)
        {
            this.uiPlayerLabel.text = text;
        }

        public void setHealthValue(float health)
        {
            this.uiHealthSlider.value = health;
        }

        public void setLoadingIndicator(float loadingProgress)
        {
            this.uiLoadingSlider.value = loadingProgress;
        }

        public void setGunAimingState(float aimValue)
        {
            this.uiFireSlider.value = aimValue;
        }

        public void showDestroyFx(Transform tankTransform)
        {
            GameObject.Instantiate(this.tankExplosionPrefab, tankTransform.position, tankTransform.rotation);
        }
    }
}