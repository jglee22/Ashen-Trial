using UnityEngine;

namespace AshenTrial
{
    [CreateAssetMenu(fileName = "Upgrade", menuName = "Ashen Trial/Upgrade")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        public enum UpgradeType { AttackDamage, MoveSpeed, MaxHealth, AttackSpeed, DodgeCooldown, ComboFinisher, Desperation }
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private UpgradeType type;
        [SerializeField, Min(0f), Tooltip("Fractional increase for damage/speed; fractional reduction for dodge cooldown; flat points for max HP.")]
        private float value;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public UpgradeType Type => type;
        public float Value => value;
    }
}
