using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class CombatFeedbackController : MonoBehaviour
    {
        [System.Serializable]
        private sealed class TargetFeedback
        {
            public Health health;
            public HitFlashFeedback flash;
        }

        [SerializeField] private TargetFeedback[] targets;
        [SerializeField] private Health playerHealth;
        [SerializeField] private GameFlowController gameFlow;
        [SerializeField] private PlayerFollowCamera followCamera;
        [SerializeField] private Camera viewCamera;
        [SerializeField] private DamageNumber damageNumberPrefab;
        [SerializeField] private GameAudio gameAudio;
        [Header("Hit Stop")]
        [SerializeField, Min(0f)] private float hitStopDuration = 0.05f;
        [Header("Camera Shake")]
        [SerializeField, Min(0f)] private float bossShakeStrength = 0.06f;
        [SerializeField, Min(0f)] private float bossShakeDuration = 0.12f;
        [SerializeField, Min(0f)] private float playerShakeStrength = 0.14f;
        [SerializeField, Min(0f)] private float playerShakeDuration = 0.18f;
        [Header("Hit Flash")]
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField, Min(0f)] private float flashDuration = 0.12f;
        [Header("Damage Number")]
        [SerializeField, Min(0.01f)] private float numberDuration = 0.7f;
        [SerializeField] private float numberHeight = 2.4f;
        [SerializeField] private float numberRise = 0.8f;
        private System.Action<float>[] handlers;
        private bool hitStopped;
        private float stopUntil;
        private float previousTimeScale;

        private void Awake()
        {
            if (playerHealth == null || gameFlow == null || followCamera == null || viewCamera == null ||
                damageNumberPrefab == null || targets == null || targets.Length == 0)
            {
                Debug.LogError("CombatFeedbackController: Health, flow, camera, number prefab and target references are required.", this);
                enabled = false;
                return;
            }
            handlers = new System.Action<float>[targets.Length];
            for (int i = 0; i < targets.Length; i++)
            {
                TargetFeedback target = targets[i];
                if (target == null || target.health == null || target.flash == null)
                {
                    Debug.LogError("CombatFeedbackController: Each target needs Health and Hit Flash.", this);
                    enabled = false;
                    return;
                }
                handlers[i] = amount => OnDamage(target, amount);
            }
        }

        private void OnEnable()
        {
            if (handlers == null) return;
            for (int i = 0; i < targets.Length; i++) targets[i].health.DamageApplied += handlers[i];
        }

        private void OnDamage(TargetFeedback target, float amount)
        {
            target.flash.Flash(flashColor, flashDuration);
            DamageNumber number = Instantiate(damageNumberPrefab,
                target.health.transform.position + Vector3.up * numberHeight, viewCamera.transform.rotation);
            number.Initialize(amount, viewCamera, numberDuration, numberRise);
            bool playerHit = target.health == playerHealth;
            if (playerHit) gameAudio?.PlayPlayerDamage();
            else gameAudio?.PlayBossHit();
            followCamera.Shake(playerHit ? playerShakeStrength : bossShakeStrength,
                playerHit ? playerShakeDuration : bossShakeDuration);
            if (playerHit || hitStopDuration <= 0f) return;
            if (!hitStopped)
            {
                previousTimeScale = Time.timeScale;
                hitStopped = true;
                Time.timeScale = 0f;
            }
            stopUntil = Mathf.Max(stopUntil, Time.unscaledTime + hitStopDuration);
        }

        private void Update()
        {
            if (hitStopped && (Time.unscaledTime >= stopUntil ||
                gameFlow.State == GameFlowController.GameFlowState.GameOver ||
                gameFlow.State == GameFlowController.GameFlowState.RunComplete)) RestoreTimeScale();
        }

        private void RestoreTimeScale()
        {
            if (!hitStopped) return;
            Time.timeScale = previousTimeScale;
            hitStopped = false;
            stopUntil = 0f;
        }

        private void OnDisable()
        {
            if (handlers != null)
                for (int i = 0; i < targets.Length; i++)
                    if (targets[i] != null && targets[i].health != null) targets[i].health.DamageApplied -= handlers[i];
            RestoreTimeScale();
        }
    }
}
