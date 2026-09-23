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
        [SerializeField] private PlayerCombat playerCombat;
        [SerializeField] private GameObject playerHitImpactPrefab;
        [Header("Hit Stop")]
        [SerializeField, Min(0f)] private float hitStopDuration = 0.05f;
        [SerializeField, Min(0f)] private float heavyHitStopDuration = 0.08f;
        [Header("Camera Shake")]
        [SerializeField, Min(0f)] private float bossShakeStrength = 0.06f;
        [SerializeField, Min(0f)] private float bossShakeDuration = 0.12f;
        [SerializeField, Min(0f)] private float heavyShakeStrength = 0.08f;
        [SerializeField, Min(0f)] private float heavyShakeDuration = 0.16f;
        [SerializeField, Min(0f)] private float playerShakeStrength = 0.14f;
        [SerializeField, Min(0f)] private float playerShakeDuration = 0.18f;
        [Header("Hit Flash")]
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField, Min(0f)] private float flashDuration = 0.12f;
        [Header("Damage Number")]
        [SerializeField, Min(0.01f)] private float numberDuration = 0.7f;
        [SerializeField] private float numberHeight = 2.4f;
        [SerializeField] private float numberRise = 0.8f;
        [Header("Hit Impact")]
        [SerializeField, Min(0.05f)] private float hitImpactLifetime = 0.4f;
        [SerializeField, Min(1f)] private float uppercutImpactScale = 1.25f;
        private const int UppercutComboStep = 3;
        private System.Action<float>[] handlers;
        private bool hitStopped;
        private float stopUntil;
        private float previousTimeScale;
        private bool pendingHitImpact;
        private Vector3 pendingHitImpactFallback;
        private float pendingHitImpactScale;

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
            bool heavyHit = !playerHit && playerCombat != null && playerCombat.IsHeavyAttacking;
            float shakeStrength = playerHit ? playerShakeStrength : heavyHit ? heavyShakeStrength : bossShakeStrength;
            float shakeDuration = playerHit ? playerShakeDuration : heavyHit ? heavyShakeDuration : bossShakeDuration;
            followCamera.Shake(shakeStrength, shakeDuration);
            if (!playerHit) QueuePlayerHitImpact(target);
            float stopDuration = heavyHit ? heavyHitStopDuration : hitStopDuration;
            if (playerHit || stopDuration <= 0f) return;
            if (!hitStopped)
            {
                previousTimeScale = Time.timeScale;
                hitStopped = true;
                Time.timeScale = 0f;
            }
            stopUntil = Mathf.Max(stopUntil, Time.unscaledTime + stopDuration);
        }

        private void Update()
        {
            if (hitStopped && (Time.unscaledTime >= stopUntil ||
                gameFlow.State == GameFlowController.GameFlowState.GameOver ||
                gameFlow.State == GameFlowController.GameFlowState.RunComplete)) RestoreTimeScale();
        }

        private void LateUpdate()
        {
            if (!pendingHitImpact) return;
            pendingHitImpact = false;
            SpawnPlayerHitImpact(ResolveHitImpactPosition(pendingHitImpactFallback), pendingHitImpactScale);
        }

        private void QueuePlayerHitImpact(TargetFeedback target)
        {
            if (playerHitImpactPrefab == null || target == null || target.health == null) return;
            pendingHitImpactFallback = ResolveHitImpactFallback(target);
            pendingHitImpactScale = playerCombat != null &&
                (playerCombat.IsHeavyAttacking || playerCombat.ComboStep == UppercutComboStep)
                ? uppercutImpactScale : 1f;
            pendingHitImpact = true;
        }

        private void SpawnPlayerHitImpact(Vector3 position, float scale)
        {
            GameObject instance = Instantiate(playerHitImpactPrefab, position, Quaternion.identity);
            instance.transform.localScale = Vector3.one * scale;
            Destroy(instance, hitImpactLifetime);
        }

        private Vector3 ResolveHitImpactPosition(Vector3 fallback)
        {
            Transform hand = playerCombat != null ? playerCombat.ImpactHand : null;
            return hand != null ? hand.position : fallback;
        }

        private Vector3 ResolveHitImpactFallback(TargetFeedback target)
        {
            if (target.health.TryGetLastHitPoint(out Vector3 hitPoint))
                return hitPoint;
            Vector3 fallback = playerHealth.transform.position;
            if (playerHealth.TryGetComponent(out CharacterController playerBody))
                fallback = playerBody.bounds.center;
            return fallback;
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
            pendingHitImpact = false;
            RestoreTimeScale();
        }
    }
}
