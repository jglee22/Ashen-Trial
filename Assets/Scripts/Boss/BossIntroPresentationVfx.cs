using UnityEngine;

namespace AshenTrial
{
    [DefaultExecutionOrder(150)]
    [DisallowMultipleComponent]
    public sealed class BossIntroPresentationVfx : MonoBehaviour
    {
        [SerializeField] private Health health;
        [SerializeField] private ParticleSystem primary;
        [SerializeField] private ParticleSystem secondary;
        [SerializeField] private PlayerFollowCamera followCamera;
        [SerializeField, Min(0f)] private float startDelay = 0.05f;
        [SerializeField, Min(0f)] private float secondaryDelay = 0.08f;
        [SerializeField, Min(0f)] private float shakeStrength = 0.08f;
        [SerializeField, Min(0f)] private float shakeDuration = 0.14f;
        private bool played;
        private Coroutine sequence;

        private void Awake()
        {
            if (health == null || primary == null || secondary == null || followCamera == null)
            {
                Debug.LogError("BossIntroPresentationVfx: Health, both particle systems and camera are required.", this);
                enabled = false;
                return;
            }
            StopAll(true);
        }

        private void OnEnable()
        {
            if (health != null) health.Died += OnDied;
        }

        private void Start()
        {
            Play();
        }

        private void OnDisable()
        {
            if (health != null) health.Died -= OnDied;
            StopSequence();
            StopAll(true);
        }

        public void Play()
        {
            if (!isActiveAndEnabled || played) return;
            if (health != null && health.IsDead) return;
            played = true;
            sequence = StartCoroutine(PlaySequence());
        }

        private void OnDied()
        {
            StopSequence();
            StopAll(true);
        }

        private System.Collections.IEnumerator PlaySequence()
        {
            if (startDelay > 0f)
            {
                float until = Time.unscaledTime + startDelay;
                while (Time.unscaledTime < until) yield return null;
            }
            if (health != null && health.IsDead)
            {
                sequence = null;
                yield break;
            }
            PlayOne(primary);
            followCamera.Shake(shakeStrength, shakeDuration);
            if (secondaryDelay > 0f)
            {
                float until = Time.unscaledTime + secondaryDelay;
                while (Time.unscaledTime < until) yield return null;
            }
            if (health != null && health.IsDead)
            {
                sequence = null;
                yield break;
            }
            PlayOne(secondary);
            sequence = null;
        }

        private void StopSequence()
        {
            if (sequence == null) return;
            StopCoroutine(sequence);
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
