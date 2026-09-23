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
        [SerializeField] private InputActionReference heavyAttackActionReference;
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
        private InputAction heavyAttackAction;
        private Transform rightHand;
        private Transform leftHand;
        private Transform rightFoot;
        private BoxCollider hitCollider;
        private Vector3 hitboxRestSize;
        private bool isHeavy;
        private float remainingTime;
        private float recoveryTime;

        public AttackPhase Phase => phase;
        public int ComboStep => comboStep;
        public bool IsHeavyAttacking => isHeavy;
        public Transform ImpactHand => isHeavy ? rightFoot : comboStep == 2 ? leftHand : rightHand;
        public float AttackDamage => config.AttackDamage * (upgrades != null
            ? upgrades.AttackDamageMultiplier * upgrades.GetDesperationMultiplier(health) : 1f);
        public float FinisherDamage => AttackDamage * (upgrades != null ? upgrades.FinisherDamageMultiplier : 1f);
        public float HeavyAttackDamage => config.HeavyAttackDamage * (upgrades != null
            ? upgrades.AttackDamageMultiplier * upgrades.GetDesperationMultiplier(health) : 1f);
        public float AttackSpeedMultiplier => upgrades != null ? upgrades.AttackSpeedMultiplier : 1f;
        public float WindupDuration => config.WindupDuration / AttackSpeedMultiplier;
        public float ActiveDuration => config.ActiveDuration / AttackSpeedMultiplier;
        public float RecoveryDuration => config.RecoveryDuration / AttackSpeedMultiplier;
        public float HeavyWindupDuration => config.HeavyWindupDuration / AttackSpeedMultiplier;
        public float HeavyActiveDuration => config.HeavyActiveDuration / AttackSpeedMultiplier;
        public float HeavyRecoveryDuration => config.HeavyRecoveryDuration / AttackSpeedMultiplier;
        public bool NextAttackQueued => nextAttackQueued;
        public bool ComboWindowOpen => comboWindowOpen;
        public bool CanAttack => isActiveAndEnabled && phase == AttackPhase.Idle &&
            (dodge == null || !dodge.IsDodging) && (health == null || !health.IsDead);

        private void Awake()
        {
            if (config == null || hitbox == null || attackActionReference == null ||
                attackActionReference.action == null ||
                attackActionReference.action.type != InputActionType.Button ||
                heavyAttackActionReference == null || heavyAttackActionReference.action == null ||
                heavyAttackActionReference.action.type != InputActionType.Button)
            {
                Debug.LogError("PlayerCombat: Config, Player/Attack, Player/HeavyAttack and Hitbox references are required.", this);
                enabled = false;
                return;
            }
            attackAction = attackActionReference.action.Clone();
            heavyAttackAction = heavyAttackActionReference.action.Clone();
            hitCollider = hitbox.GetComponent<BoxCollider>();
            if (hitCollider != null) hitboxRestSize = hitCollider.size;
            CacheImpactHands();
            ResetCombo();
        }

        private void CacheImpactHands()
        {
            Animator animator = GetComponentInChildren<Animator>(true);
            if (animator == null || !animator.isHuman) return;
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }

        private Transform ResolveImpactHand()
        {
            return comboStep == 2 ? leftHand : rightHand;
        }

        private void OnEnable()
        {
            attackAction?.Enable();
            heavyAttackAction?.Enable();
        }

        private void OnDisable()
        {
            attackAction?.Disable();
            heavyAttackAction?.Disable();
            ResetCombo();
        }

        private void ResetCombo()
        {
            if (hitbox != null) hitbox.SetActive(false);
            RestoreHitboxSize();
            phase = AttackPhase.Idle;
            remainingTime = 0f;
            recoveryTime = 0f;
            comboStep = 0;
            isHeavy = false;
            nextAttackQueued = false;
            comboWindowOpen = false;
        }

        private void OnDestroy()
        {
            attackAction?.Dispose();
            heavyAttackAction?.Dispose();
        }

        private void Update()
        {
            if (attackAction == null || heavyAttackAction == null) return;
            TickAttack(Time.deltaTime, attackAction.WasPressedThisFrame(),
                heavyAttackAction.WasPressedThisFrame());
        }

        private void StartAttack(int step)
        {
            comboStep = step;
            isHeavy = false;
            nextAttackQueued = false;
            comboWindowOpen = false;
            RestoreHitboxSize();
            hitbox.BeginSwing(step == MaxComboStep ? FinisherDamage : AttackDamage, transform,
                ResolveImpactHand());
            phase = AttackPhase.Windup;
            remainingTime = WindupDuration;
        }

        private void StartHeavy()
        {
            comboStep = 0;
            isHeavy = true;
            nextAttackQueued = false;
            comboWindowOpen = false;
            ApplyHeavyHitboxSize();
            hitbox.BeginSwing(HeavyAttackDamage, transform, rightFoot);
            phase = AttackPhase.Windup;
            remainingTime = HeavyWindupDuration;
        }

        private void ApplyHeavyHitboxSize()
        {
            if (hitCollider == null) return;
            hitCollider.size = hitboxRestSize * config.HeavyHitboxScale;
        }

        private void RestoreHitboxSize()
        {
            if (hitCollider == null) return;
            hitCollider.size = hitboxRestSize;
        }

        private void TickAttack(float deltaTime, bool pressed, bool heavyPressed)
        {
            if (phase == AttackPhase.Idle)
            {
                if (heavyPressed && CanAttack) StartHeavy();
                else if (pressed && CanAttack) StartAttack(1);
                return;
            }

            remainingTime -= deltaTime;
            float recoveryProgress = recoveryTime > 0f ? 1f - remainingTime / recoveryTime : 1f;
            comboWindowOpen = !isHeavy && phase == AttackPhase.Recovery && comboStep < MaxComboStep &&
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
                    remainingTime = isHeavy ? HeavyActiveDuration : ActiveDuration;
                    gameAudio?.PlayPlayerSwing(isHeavy ? 1 : comboStep);
                    hitbox.SetActive(true);
                    break;
                case AttackPhase.Active:
                    hitbox.SetActive(false);
                    phase = AttackPhase.Recovery;
                    recoveryTime = isHeavy ? HeavyRecoveryDuration : RecoveryDuration;
                    remainingTime = recoveryTime;
                    break;
                case AttackPhase.Recovery:
                    if (!isHeavy && nextAttackQueued && comboStep < MaxComboStep)
                        StartAttack(comboStep + 1);
                    else
                        ResetCombo();
                    break;
            }
        }
    }
}
