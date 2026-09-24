using UnityEngine;

namespace AshenTrial
{
    [DefaultExecutionOrder(150)]
    [DisallowMultipleComponent]
    public sealed class BossDeathPresentationVfx : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private ParticleSystem primary;
        [SerializeField] private ParticleSystem secondary;
        [SerializeField] private PlayerFollowCamera followCamera;
        [SerializeField, Min(0f)] private float startDelay = 0.1f;
        [SerializeField, Min(0f)] private float secondaryDelay = 0.1f;
        [SerializeField, Min(0f)] private float shakeStrength = 0.16f;
        [SerializeField, Min(0f)] private float shakeDuration = 0.2f;
        private bool played;
        private Coroutine sequence;

        private void Awake()
        {
            if (health == null || primary == null || secondary == null || followCamera == null)
            {
                Debug.LogError("BossDeathPresentationVfx: Health, both particle systems and camera are required.", this);
                enabled = false;
                return;
            }
            StopAll(true);
        }

        private void OnEnable()
        {
            if (health != null) health.Died += Play;
        }

        private void OnDisable()
        {
            if (health != null) health.Died -= Play;
            if (sequence != null)
            {
                StopCoroutine(sequence);
                sequence = null;
            }
            StopAll(true);
        }

        public void Play()
        {
            if (!isActiveAndEnabled || played) return;
            if (health != null && !health.IsDead) return;
            played = true;
            sequence = StartCoroutine(PlaySequence());
        }

        private System.Collections.IEnumerator PlaySequence()
        {
            if (startDelay > 0f)
            {
                float until = Time.unscaledTime + startDelay;
                while (Time.unscaledTime < until) yield return null;
            }
            PlayOne(primary);
            followCamera.Shake(shakeStrength, shakeDuration);
            if (secondaryDelay > 0f)
            {
                float until = Time.unscaledTime + secondaryDelay;
                while (Time.unscaledTime < until) yield return null;
            }
            PlayOne(secondary);
            sequence = null;
        }

        private static void PlayOne(ParticleSystem system)
        {
            if (system == null) return;
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            system.Play(true);
        }

        private void StopAll(bool clear)
        {
            StopOne(primary, clear);
            StopOne(secondary, clear);
        }

        private static void StopOne(ParticleSystem system, bool clear)
        {
            if (system == null) return;
            system.Stop(true, clear
                ? ParticleSystemStopBehavior.StopEmittingAndClear
                : ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
