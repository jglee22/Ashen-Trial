using UnityEngine;

namespace AshenTrial
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class PlayerAnimationDriver : MonoBehaviour
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int ActionSpeed = Animator.StringToHash("ActionSpeed");
        private static readonly int Locomotion = Animator.StringToHash("Base Layer.Locomotion");
        private static readonly int Hit = Animator.StringToHash("Base Layer.Hit");
        private static readonly int Death = Animator.StringToHash("Base Layer.Death");
        private static readonly int[] Attacks = { Animator.StringToHash("Base Layer.Attack1"),
            Animator.StringToHash("Base Layer.Attack2"), Animator.StringToHash("Base Layer.Attack3") };
        private static readonly int HeavyAttack = Animator.StringToHash("Base Layer.HeavyAttack");
        private static readonly int[] Rolls = { Animator.StringToHash("Base Layer.RollForward"),
            Animator.StringToHash("Base Layer.RollBackward"), Animator.StringToHash("Base Layer.RollLeft"),
            Animator.StringToHash("Base Layer.RollRight") };

        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private PlayerDodge dodge;
        [SerializeField] private Health health;
        [SerializeField] private AnimationClip[] attackClips;
        [SerializeField] private AnimationClip[] rollClips;
        [SerializeField] private AnimationClip hitClip;
        [SerializeField] private AnimationClip heavyClip;
        [SerializeField, Min(0f)] private float locomotionBlend = 0.06f;
        [SerializeField, Min(0.01f)] private float rollVisualDuration = 0.4f;
        [SerializeField] private float[] attackImpactTimes;
        [SerializeField] private float[] attackRecoveryTimes;
        [SerializeField, Range(0.01f, 0.98f)] private float heavyImpactTime = 0.32f;
        [SerializeField, Range(0.02f, 0.99f)] private float heavyRecoveryTime = 0.62f;
        private int currentState;
        private int previousCombo;
        private bool wasDodging;
        private bool wasHeavy;
        private int rollIndex;
        private float hitUntil;
        private float rollUntil;
        private PlayerCombat.AttackPhase animationAttackPhase;

        private void Awake()
        {
            if (animator == null || characterController == null || movement == null || combat == null ||
                dodge == null || health == null || hitClip == null || heavyClip == null ||
                attackClips == null || attackClips.Length != 3 ||
                rollClips == null || rollClips.Length != 4 || attackImpactTimes == null || attackImpactTimes.Length != 3 ||
                attackRecoveryTimes == null || attackRecoveryTimes.Length != 3 ||
                System.Array.Exists(attackClips, c => c == null) ||
                System.Array.Exists(rollClips, c => c == null) || animator.runtimeAnimatorController == null)
            {
                Debug.LogError("PlayerAnimationDriver: Animator, gameplay and animation references are required.", this);
                enabled = false;
                return;
            }
            animator.applyRootMotion = false;
        }

        private void OnEnable()
        {
            if (health != null) health.DamageApplied += OnDamageApplied;
        }

        private void OnDisable()
        {
            if (health != null) health.DamageApplied -= OnDamageApplied;
            rollUntil = 0f;
            wasDodging = false;
            wasHeavy = false;
        }

        private void OnDamageApplied(float damage)
        {
            if (health.IsDead || damage <= 0f) return;
            rollUntil = 0f;
            hitUntil = Time.time + hitClip.length;
            // The reaction can be interrupted by a newly accepted action; it never locks gameplay.
            currentState = 0;
        }

        private void Update()
        {
            if (health.IsDead)
            {
                rollUntil = 0f;
                SetState(Death, 1f);
                return;
            }

            Vector3 velocity = characterController.enabled && movement.isActiveAndEnabled
                ? characterController.velocity : Vector3.zero;
            velocity.y = 0f;
            Vector3 local = transform.InverseTransformDirection(velocity) / Mathf.Max(0.01f, movement.MoveSpeed);
            animator.SetFloat(MoveX, local.x, locomotionBlend, Time.deltaTime);
            animator.SetFloat(MoveY, local.z, locomotionBlend, Time.deltaTime);

            bool newAttack = combat.IsHeavyAttacking
                ? !wasHeavy
                : combat.ComboStep != 0 && combat.ComboStep != previousCombo;
            bool newDodge = dodge.IsDodging && !wasDodging;
            if (newAttack || newDodge) hitUntil = 0f;
            if (newAttack) rollUntil = 0f;
            previousCombo = combat.ComboStep;
            wasDodging = dodge.IsDodging;
            wasHeavy = combat.IsHeavyAttacking;

            if (newDodge)
            {
                rollUntil = Time.time + Mathf.Max(0.01f, rollVisualDuration);
                currentState = 0;
                Vector3 direction = transform.InverseTransformDirection(dodge.CurrentDodgeDirection);
                rollIndex = Mathf.Abs(direction.z) >= Mathf.Abs(direction.x)
                    ? (direction.z >= 0f ? 0 : 1) : (direction.x < 0f ? 2 : 3);
            }
            if (Time.time < hitUntil)
                SetState(Hit, 1f);
            else if (Time.time < rollUntil)
                SetState(Rolls[rollIndex], rollClips[rollIndex].length / Mathf.Max(0.01f, rollVisualDuration));
            else if (combat.IsHeavyAttacking)
                PlayMappedAttack(HeavyAttack, heavyClip, heavyImpactTime, heavyRecoveryTime,
                    combat.HeavyWindupDuration, combat.HeavyActiveDuration, combat.HeavyRecoveryDuration);
            else if (combat.ComboStep > 0)
            {
                int index = combat.ComboStep - 1;
                PlayMappedAttack(Attacks[index], attackClips[index], attackImpactTimes[index],
                    attackRecoveryTimes[index], combat.WindupDuration, combat.ActiveDuration,
                    combat.RecoveryDuration);
            }
            else
                SetState(Locomotion, 1f);
        }

        private void PlayMappedAttack(int state, AnimationClip clip, float impactTime, float recoveryTime,
            float windupDuration, float activeDuration, float recoveryDuration)
        {
            float impact = Mathf.Clamp(impactTime, 0.01f, 0.98f);
            float recovery = Mathf.Clamp(recoveryTime, impact + 0.01f, 0.99f);
            float from = combat.Phase == PlayerCombat.AttackPhase.Windup ? 0f :
                combat.Phase == PlayerCombat.AttackPhase.Active ? impact : recovery;
            float to = combat.Phase == PlayerCombat.AttackPhase.Windup ? impact :
                combat.Phase == PlayerCombat.AttackPhase.Active ? recovery : 1f;
            float duration = combat.Phase == PlayerCombat.AttackPhase.Windup ? windupDuration :
                combat.Phase == PlayerCombat.AttackPhase.Active ? activeDuration : recoveryDuration;
            // Match the contact pose to the gameplay phase, including FURY and hit stop.
            if (currentState != state || animationAttackPhase != combat.Phase)
                animator.Play(state, 0, from);
            currentState = state;
            animationAttackPhase = combat.Phase;
            animator.SetFloat(ActionSpeed, clip.length * (to - from) / Mathf.Max(0.0001f, duration));
        }

        private void SetState(int state, float speed)
        {
            animator.SetFloat(ActionSpeed, speed);
            if (currentState == state) return;
            if (state == Locomotion) animator.CrossFadeInFixedTime(state, locomotionBlend, 0);
            else animator.Play(state, 0, 0f);
            currentState = state;
        }
    }
}
