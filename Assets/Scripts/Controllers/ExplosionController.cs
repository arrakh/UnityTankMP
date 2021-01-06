using UnityEngine;

namespace UnityTank
{
    [RequireComponent(typeof(AudioSource))]
    public class ExplosionController : MonoBehaviour
    {
        public float lifeTime = 1.0f;
        private void Start ()
        {
            AudioSource audio = GetComponent<AudioSource>();
            audio.loop = false;
            audio.Play();
            ParticleSystem[] particles = GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < particles.Length; ++i)
            {
                particles[i].Play();
            }            
            Destroy (gameObject, lifeTime);
        }

        private void Update()
        {
            Light[] lights = GetComponentsInChildren<Light>(true);
 
            foreach (Light light in lights)
            {
                light.intensity = Mathf.Clamp(light.intensity-Time.deltaTime,0.0f,2.0f);
            }
        }
    }
}