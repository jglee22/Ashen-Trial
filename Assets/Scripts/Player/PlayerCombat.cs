using UnityEngine;
using UnityEngine.InputSystem;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class PlayerCombat : MonoBehaviour
    {
        public enum AttackPhase { Idle, Windup, Active, Recovery }
        private const int MaxComboStep = 3;

        [SerializeField] private PlayerConfig config;
        [SerializeField] private InputActionReference attackActionReference;
        [SerializeField] private AttackHitbox hitbox;
        [SerializeField] private PlayerDodge dodge;
        [SerializeField] private Health health;
        [SerializeField] private PlayerUpgradeState upgrades;
        [SerializeField] private GameAudio gameAudio;
        [SerializeField] private AttackPhase phase;
        [SerializeField] private int comboStep;
        [SerializeField] private bool nextAttackQueued;
        [SerializeField] private bool comboWindowOpen;
        private InputAction attackAction;
        private float remainingTime;
        private float recoveryTime;

        public AttackPhase Phase => phase;
        public int ComboStep => comboStep;
        public float AttackDamage => config.AttackDamage * (upgrades != null ? upgrades.AttackDamageMultiplier : 1f);
        public float FinisherDamage => AttackDamage * (upgrades != null ? upgrades.FinisherDamageMultiplier : 1f);
        public float AttackSpeedMultiplier => upgrades != null ? upgrades.AttackSpeedMultiplier : 1f;
        public float WindupDuration => config.WindupDuration / AttackSpeedMultiplier;
        public float ActiveDuration => config.ActiveDuration / AttackSpeedMultiplier;
        public float RecoveryDuration => config.RecoveryDuration / AttackSpeedMultiplier;
        public bool NextAttackQueued => nextAttackQueued;
        public bool ComboWindowOpen => comboWindowOpen;
        public bool CanAttack => isActiveAndEnabled && phase == AttackPhase.Idle &&
            (dodge == null || !dodge.IsDodging) && (health == null || !health.IsDead);

        private void Awake()
        {
            if (config == null || hitbox == null || attackActionReference == null ||
                attackActionReference.action == null ||
                attackActionReference.action.type != InputActionType.Button)
            {
                Debug.LogError("PlayerCombat: Config, Player/Attack and Hitbox references are required.", this);
                enabled = false;
                return;
            }
            attackAction = attackActionReference.action.Clone();
            ResetCombo();
        }

        private void OnEnable() => attackAction?.Enable();

        private void OnDisable()
        {
            attackAction?.Disable();
            ResetCombo();
        }

        private void ResetCombo()
        {
            if (hitbox != null) hitbox.SetActive(false);
            phase = AttackPhase.Idle;
            remainingTime = 0f;
            recoveryTime = 0f;
            comboStep = 0;
            nextAttackQueued = false;
            comboWindowOpen = false;
        }

        private void OnDestroy() => attackAction?.Dispose();

        private void Update()
        {
            if (attackAction == null) return;
            TickAttack(Time.deltaTime, attackAction.WasPressedThisFrame());
        }

        private void StartAttack(int step)
        {
            comboStep = step;
            nextAttackQueued = false;
            comboWindowOpen = false;
            hitbox.BeginSwing(step == MaxComboStep ? FinisherDamage : AttackDamage, transform);
            phase = AttackPhase.Windup;
            remainingTime = WindupDuration;
        }

        private void TickAttack(float deltaTime, bool pressed)
        {
            if (phase == AttackPhase.Idle)
            {
                if (pressed && CanAttack) StartAttack(1);
                return;
            }

            remainingTime -= deltaTime;
            float recoveryProgress = recoveryTime > 0f ? 1f - remainingTime / recoveryTime : 1f;
            comboWindowOpen = phase == AttackPhase.Recovery && comboStep < MaxComboStep &&
                remainingTime > 0f && recoveryProgress >= config.ComboWindowStart &&
                recoveryProgress < config.ComboWindowEnd;
            if (pressed && comboWindowOpen) nextAttackQueued = true;
            if (remainingTime > 0f) return;
            comboWindowOpen = false;
            // One transition per frame keeps every phase observable even during a long frame.
            switch (phase)
            {
                case AttackPhase.Windup:
                    phase = AttackPhase.Active;
                    remainingTime = ActiveDuration;
                    gameAudio?.PlayPlayerSwing(comboStep);
                    hitbox.SetActive(true);
                    break;
                case AttackPhase.Active:
                    hitbox.SetActive(false);
                    phase = AttackPhase.Recovery;
                    recoveryTime = RecoveryDuration;
                    remainingTime = recoveryTime;
                    break;
                case AttackPhase.Recovery:
                    if (nextAttackQueued && comboStep < MaxComboStep)
                        StartAttack(comboStep + 1);
                    else
                        ResetCombo();
                    break;
            }
        }
    }
}
