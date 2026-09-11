using UnityEngine;

namespace AshenTrial
{
    [DefaultExecutionOrder(-200)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Health))]
    public sealed class PlayerVitals : MonoBehaviour
    {
        [SerializeField] private PlayerConfig config;
        [SerializeField] private Health health;
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private PlayerDodge dodge;
        private float hitInvincibleUntil;

        public bool HitInvincible => Time.time < hitInvincibleUntil;
        public bool IsInvincible => HitInvincible || (dodge != null && dodge.IsDodging);

        private void Awake()
        {
            if (config == null || health == null || movement == null || combat == null || dodge == null)
            {
                Debug.LogError("PlayerVitals: Missing references.", this);
                enabled = false;
                return;
            }
            health.Initialize(config.MaxHealth);
        }

        private void OnEnable()
        {
            if (health == null) return;
            health.DamageBlocked = BlocksDamage;
            health.Damaged += OnDamaged;
            health.Died += OnDeath;
        }

        private void OnDisable()
        {
            if (health == null) return;
            health.DamageBlocked = null;
            health.Damaged -= OnDamaged;
            health.Died -= OnDeath;
            hitInvincibleUntil = 0f;
        }

        private bool BlocksDamage() => IsInvincible;
        private void OnDamaged() => hitInvincibleUntil = Time.time + config.HitInvincibilityDuration;

        private void OnDeath()
        {
            combat.enabled = false;
            dodge.enabled = false;
            movement.enabled = false;
        }
    }
}
