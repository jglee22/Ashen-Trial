using UnityEngine;

namespace AshenTrial
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "Ashen Trial/Player Config")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [SerializeField, Min(0f)] private float moveSpeed = 6f;
        [SerializeField, Min(0f)] private float rotationSpeed = 720f;
        [Header("Basic Attack")]
        [SerializeField, Min(0f)] private float attackDamage = 25f;
        [SerializeField, Min(0f)] private float windupDuration = 0.2f;
        [SerializeField, Min(0.01f)] private float activeDuration = 0.15f;
        [SerializeField, Min(0f)] private float recoveryDuration = 0.35f;
        [Header("Combo Window (Recovery normalized time)")]
        [SerializeField, Range(0f, 1f)] private float comboWindowStart = 0.15f;
        [SerializeField, Range(0f, 1f)] private float comboWindowEnd = 0.85f;
        [Header("Health and Dodge")]
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float hitInvincibilityDuration = 0.75f;
        [SerializeField, Min(0f)] private float dodgeDistance = 3f;
        [SerializeField, Min(0.01f)] private float dodgeDuration = 0.25f;
        [SerializeField, Min(0f)] private float dodgeCooldown = 0.6f;

        public float MoveSpeed => Mathf.Max(0f, moveSpeed);
        public float RotationSpeed => Mathf.Max(0f, rotationSpeed);
        public float AttackDamage => Mathf.Max(0f, attackDamage);
        public float WindupDuration => Mathf.Max(0f, windupDuration);
        public float ActiveDuration => Mathf.Max(0.01f, activeDuration);
        public float RecoveryDuration => Mathf.Max(0f, recoveryDuration);
        public float ComboWindowStart => Mathf.Clamp01(comboWindowStart);
        public float ComboWindowEnd => Mathf.Clamp(comboWindowEnd, ComboWindowStart, 1f);
        public float MaxHealth => Mathf.Max(1f, maxHealth);
        public float HitInvincibilityDuration => Mathf.Max(0f, hitInvincibilityDuration);
        public float DodgeDistance => Mathf.Max(0f, dodgeDistance);
        public float DodgeDuration => Mathf.Max(0.01f, dodgeDuration);
        public float DodgeCooldown => Mathf.Max(0f, dodgeCooldown);
    }
}
