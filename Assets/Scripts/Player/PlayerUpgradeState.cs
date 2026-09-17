using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class PlayerUpgradeState : MonoBehaviour
    {
        [SerializeField] private Health health;
        public float AttackDamageMultiplier { get; private set; } = 1f;
        public float MoveSpeedMultiplier { get; private set; } = 1f;
        public float AttackSpeedMultiplier { get; private set; } = 1f;
        public float DodgeCooldownMultiplier { get; private set; } = 1f;
        public float FinisherDamageMultiplier { get; private set; } = 1f;
        public float DesperationDamageMultiplier { get; private set; } = 1f;
        public float MaxHealthBonus { get; private set; }
        public bool HasDesperation { get; private set; }
        public UpgradeDefinition SelectedUpgrade { get; private set; }
        private const float DesperationHealthThreshold = 0.5f;

        private void Awake()
        {
            if (health != null) return;
            Debug.LogError("PlayerUpgradeState: Health reference is required.", this);
            enabled = false;
        }

        public bool TryApply(UpgradeDefinition upgrade)
        {
            if (!isActiveAndEnabled) return false;
            if (upgrade == null || upgrade.Value <= 0f || float.IsNaN(upgrade.Value) || float.IsInfinity(upgrade.Value))
            {
                Debug.LogError("PlayerUpgradeState: A valid upgrade with a finite positive value is required.", this);
                return false;
            }
            switch (upgrade.Type)
            {
                case UpgradeDefinition.UpgradeType.AttackDamage:
                    AttackDamageMultiplier *= 1f + upgrade.Value;
                    break;
                case UpgradeDefinition.UpgradeType.MoveSpeed:
                    MoveSpeedMultiplier *= 1f + upgrade.Value;
                    break;
                case UpgradeDefinition.UpgradeType.MaxHealth:
                    health.IncreaseMaxHealth(upgrade.Value);
                    MaxHealthBonus += upgrade.Value;
                    break;
                case UpgradeDefinition.UpgradeType.AttackSpeed:
                    AttackSpeedMultiplier *= 1f + upgrade.Value;
                    break;
                case UpgradeDefinition.UpgradeType.DodgeCooldown:
                    if (upgrade.Value >= 1f)
                    {
                        Debug.LogError("PlayerUpgradeState: Dodge cooldown reduction must be less than 100%.", this);
                        return false;
                    }
                    DodgeCooldownMultiplier *= 1f - upgrade.Value;
                    break;
                case UpgradeDefinition.UpgradeType.ComboFinisher:
                    FinisherDamageMultiplier *= 1f + upgrade.Value;
                    break;
                case UpgradeDefinition.UpgradeType.Desperation:
                    HasDesperation = true;
                    DesperationDamageMultiplier = 1f + upgrade.Value;
                    break;
                default:
                    Debug.LogError("PlayerUpgradeState: Unsupported upgrade type.", this);
                    return false;
            }
            SelectedUpgrade = upgrade;
            return true;
        }

        public float CurrentDesperationMultiplier => GetDesperationMultiplier(health);

        public float GetDesperationMultiplier(Health targetHealth)
        {
            if (!HasDesperation || targetHealth == null) return 1f;
            if (targetHealth.CurrentHp > targetHealth.MaxHp * DesperationHealthThreshold) return 1f;
            return DesperationDamageMultiplier;
        }
    }
}
