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
        [SerializeField, Min(0f)] private float locomotionBlend = 0.06f;
        private int currentState;
        private int previousCombo;
        private bool wasDodging;
        private int rollIndex;
        private float hitUntil;

        private void Awake()
        {
            if (animator == null || characterController == null || movement == null || combat == null ||
                dodge == null || health == null || hitClip == null || attackClips == null || attackClips.Length != 3 ||
                rollClips == null || rollClips.Length != 4 || System.Array.Exists(attackClips, c => c == null) ||
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
        }

        private void OnDamageApplied(float damage)
        {
            if (health.IsDead || damage <= 0f) return;
            hitUntil = Time.time + hitClip.length;
            // The reaction can be interrupted by a newly accepted action; it never locks gameplay.
            currentState = 0;
        }

        private void Update()
        {
            if (health.IsDead)
            {
                SetState(Death, 1f);
                return;
            }

            Vector3 velocity = characterController.enabled && movement.isActiveAndEnabled
                ? characterController.velocity : Vector3.zero;
            velocity.y = 0f;
            Vector3 local = transform.InverseTransformDirection(velocity) / Mathf.Max(0.01f, movement.MoveSpeed);
            animator.SetFloat(MoveX, local.x, locomotionBlend, Time.deltaTime);
            animator.SetFloat(MoveY, local.z, locomotionBlend, Time.deltaTime);

            bool newAttack = combat.ComboStep != 0 && combat.ComboStep != previousCombo;
            bool newDodge = dodge.IsDodging && !wasDodging;
            if (newAttack || newDodge) hitUntil = 0f;
            previousCombo = combat.ComboStep;
            wasDodging = dodge.IsDodging;

            if (newDodge)
            {
                Vector3 direction = transform.InverseTransformDirection(dodge.CurrentDodgeDirection);
                rollIndex = Mathf.Abs(direction.z) >= Mathf.Abs(direction.x)
                    ? (direction.z >= 0f ? 0 : 1) : (direction.x < 0f ? 2 : 3);
            }
            if (Time.time < hitUntil)
                SetState(Hit, 1f);
            else if (dodge.IsDodging)
                SetState(Rolls[rollIndex], rollClips[rollIndex].length / dodge.DodgeDuration);
            else if (combat.ComboStep > 0)
            {
                int index = combat.ComboStep - 1;
                float duration = combat.WindupDuration + combat.ActiveDuration + combat.RecoveryDuration;
                SetState(Attacks[index], attackClips[index].length / duration);
            }
            else
                SetState(Locomotion, 1f);
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
