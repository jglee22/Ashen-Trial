using UnityEngine;
using UnityEngine.Localization.Settings;

namespace AshenTrial
{
    [CreateAssetMenu(fileName = "Upgrade", menuName = "Ashen Trial/Upgrade")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        public enum UpgradeType { AttackDamage, MoveSpeed, MaxHealth, AttackSpeed, DodgeCooldown, ComboFinisher, Desperation }
        private const string UpgradeTable = "Upgrade";
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private UpgradeType type;
        [SerializeField, Min(0f), Tooltip("Fractional increase for damage/speed; fractional reduction for dodge cooldown; flat points for max HP.")]
        private float value;

        public string Id => id;
        public string DisplayName => GetLocalized($"upgrade.{id}.name", displayName);
        public string Description => GetLocalized($"upgrade.{id}.description", description);
        public UpgradeType Type => type;
        public float Value => value;

        private static string GetLocalized(string key, string fallback)
        {
            if (string.IsNullOrEmpty(key) || LocalizationSettings.AvailableLocales == null ||
                LocalizationSettings.SelectedLocale == null)
                return fallback;
            string value = LocalizationSettings.StringDatabase.GetLocalizedString(UpgradeTable, key);
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}
