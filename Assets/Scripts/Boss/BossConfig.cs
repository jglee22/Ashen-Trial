using UnityEngine;

namespace AshenTrial
{
    [CreateAssetMenu(fileName = "BossConfig", menuName = "Ashen Trial/Boss Config")]
    public sealed class BossConfig : ScriptableObject
    {
        [Header("General")]
        [SerializeField, Min(1f)] private float maxHealth = 300f;
        [SerializeField, Min(0f)] private float moveSpeed = 2f;
        [SerializeField, Min(0f)] private float rotationSpeed = 180f;
        [Header("Melee Combo")]
        [SerializeField, Min(0.1f)] private float attackRange = 2f;
        [SerializeField, Min(0f)] private float attackDamage = 25f;
        [SerializeField, Min(0f)] private float attackWindup = 0.65f;
        [SerializeField, Min(0.01f)] private float attackActiveDuration = 0.2f;
        [SerializeField, Min(0.01f)] private float attackRecovery = 1.2f;
        [SerializeField, Min(0.01f)] private float meleeInterval = 0.65f;
        [Header("Charge")]
        [SerializeField, Min(0.1f)] private float chargeTriggerRange = 8f;
        [SerializeField, Min(0.01f)] private float chargeSpeed = 12f;
        [SerializeField, Min(0.01f)] private float chargeDistance = 6f;
        [SerializeField, Min(0f)] private float chargeWindup = 0.8f;
        [SerializeField, Min(0f)] private float chargeLockLeadTime = 0.5f;
        [SerializeField, Min(0.01f)] private float chargeRecovery = 1.2f;
        [SerializeField, Min(0f)] private float chargeDamage = 25f;
        [SerializeField, Min(0f)] private float chargeCooldown = 5f;
        [Header("Circle AoE")]
        [SerializeField, Min(0.1f)] private float aoeRadius = 4f;
        [SerializeField, Min(0f)] private float aoeWindup = 1f;
        [SerializeField, Min(0.01f)] private float aoeRecovery = 1.2f;
        [SerializeField, Min(0f)] private float aoeDamage = 25f;
        [SerializeField, Min(0f)] private float aoeCooldown = 5f;
        [Header("Phase Two")]
        [SerializeField, Range(0f, 1f)] private float phaseTwoHealthRatio = 0.5f;
        [SerializeField, Min(0.01f)] private float phaseTwoMeleeRecovery = 0.9f;
        [SerializeField, Min(0.01f)] private float phaseTwoChargeSpeed = 15f;
        [SerializeField, Min(0.1f)] private float phaseTwoAoERadius = 4.5f;
        [SerializeField, Min(0f)] private float phaseTwoAoECooldown = 4f;

        public float MaxHealth => Mathf.Max(1f, maxHealth);
        public float MoveSpeed => Mathf.Max(0f, moveSpeed);
        public float RotationSpeed => Mathf.Max(0f, rotationSpeed);
        public float AttackRange => Mathf.Max(0.1f, attackRange);
        public float AttackDamage => Mathf.Max(0f, attackDamage);
        public float AttackWindup => Mathf.Max(0f, attackWindup);
        public float AttackActiveDuration => Mathf.Max(0.01f, attackActiveDuration);
        public float AttackRecovery => Mathf.Max(0.01f, attackRecovery);
        public float MeleeInterval => Mathf.Max(0.01f, meleeInterval);
        public float ChargeTriggerRange => Mathf.Max(AttackRange, chargeTriggerRange);
        public float ChargeSpeed => Mathf.Max(0.01f, chargeSpeed);
        public float ChargeDistance => Mathf.Max(0.01f, chargeDistance);
        public float ChargeWindup => Mathf.Max(0f, chargeWindup);
        public float ChargeLockLeadTime => Mathf.Clamp(chargeLockLeadTime, 0f, ChargeWindup);
        public float ChargeRecovery => Mathf.Max(0.01f, chargeRecovery);
        public float ChargeDamage => Mathf.Max(0f, chargeDamage);
        public float ChargeCooldown => Mathf.Max(0f, chargeCooldown);
        public float AoERadius => Mathf.Max(0.1f, aoeRadius);
        public float AoEWindup => Mathf.Max(0f, aoeWindup);
        public float AoERecovery => Mathf.Max(0.01f, aoeRecovery);
        public float AoEDamage => Mathf.Max(0f, aoeDamage);
        public float AoECooldown => Mathf.Max(0f, aoeCooldown);
        public float PhaseTwoHealthRatio => Mathf.Clamp01(phaseTwoHealthRatio);
        public float PhaseTwoMeleeRecovery => Mathf.Max(0.01f, phaseTwoMeleeRecovery);
        public float PhaseTwoChargeSpeed => Mathf.Max(0.01f, phaseTwoChargeSpeed);
        public float PhaseTwoAoERadius => Mathf.Max(0.1f, phaseTwoAoERadius);
        public float PhaseTwoAoECooldown => Mathf.Max(0f, phaseTwoAoECooldown);
    }
}
