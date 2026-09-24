using UnityEngine;

namespace AshenTrial
{
    [DefaultExecutionOrder(150)]
    [DisallowMultipleComponent]
    public sealed class BossPhaseTransitionVfx : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private ParticleSystem primary;
        [SerializeField] private ParticleSystem secondary;
        [SerializeField] private PlayerFollowCamera followCamera;
        [SerializeField, Min(0f)] private float shakeStrength = 0.1f;
        [SerializeField, Min(0f)] private float shakeDuration = 0.18f;
        private bool played;

        private void Awake()
        {
            if (health == null || primary == null || secondary == null || followCamera == null)
            {
                Debug.LogError("BossPhaseTransitionVfx: Health, both particle systems and camera are required.", this);
                enabled = false;
                return;
            }
            StopAll(true);
        }

        private void OnDisable()
        {
            StopAll(true);
        }

        public void Play()
        {
            if (!isActiveAndEnabled || played) return;
            if (health != null && health.IsDead) return;
            played = true;
            PlayOne(primary);
            PlayOne(secondary);
            followCamera.Shake(shakeStrength, shakeDuration);
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
