using UnityEngine;

namespace AshenTrial
{
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public sealed class BossAnimationDriver : MonoBehaviour
    {
        private static readonly int MoveX = Animator.StringToHash("MoveX");
        private static readonly int MoveY = Animator.StringToHash("MoveY");
        private static readonly int ActionTime = Animator.StringToHash("ActionTime");
        private static readonly int MotionSpeed = Animator.StringToHash("MotionSpeed");
        private static readonly int Locomotion = Animator.StringToHash("Base Layer.Locomotion");
        private static readonly int RushReady = Animator.StringToHash("Base Layer.RushReady");
        private static readonly int Rush = Animator.StringToHash("Base Layer.Rush");
        private static readonly int Cast = Animator.StringToHash("Base Layer.Cast");
        private static readonly int GroundCast = Animator.StringToHash("Base Layer.GroundCast");
        private static readonly int Death = Animator.StringToHash("Base Layer.Death");
        private static readonly int[] Melee = { Animator.StringToHash("Base Layer.Melee1"),
            Animator.StringToHash("Base Layer.Melee2"), Animator.StringToHash("Base Layer.Melee3") };

        [SerializeField] private Animator animator;
        [SerializeField] private CharacterController body;
        [SerializeField] private Health health;
        [SerializeField] private BossController boss01;
        [SerializeField] private Boss02Controller boss02;
        [SerializeField] private Boss03Controller boss03;
        [SerializeField, Min(0.01f)] private float rushClipSpeed = 10f;
        [SerializeField, Min(0f)] private float movementBlend = 0.06f;
        private int currentAnimation;
        private int currentSegment = -1;
        private float segmentDuration;

        private void Awake()
        {
            int sources = (boss01 != null ? 1 : 0) + (boss02 != null ? 1 : 0) + (boss03 != null ? 1 : 0);
            if (animator == null || body == null || health == null || sources != 1 ||
                animator.runtimeAnimatorController == null)
            {
                Debug.LogError("BossAnimationDriver: Animator, body, health and exactly one boss controller are required.", this);
                enabled = false;
                return;
            }
            animator.applyRootMotion = false;
        }

        private void OnEnable()
        {
            currentAnimation = 0;
            currentSegment = -1;
        }

        private void Update()
        {
            if (health.IsDead)
            {
                PlayLoop(Death);
                return;
            }

            Vector3 velocity = body.enabled ? body.velocity : Vector3.zero;
            velocity.y = 0f;
            Vector3 local = transform.InverseTransformDirection(velocity);
            animator.SetFloat(MoveX, local.x, movementBlend, Time.deltaTime);
            animator.SetFloat(MoveY, local.z, movementBlend, Time.deltaTime);
            if (boss01 != null) UpdateBoss01(velocity.magnitude);
            else if (boss02 != null) UpdateBoss02();
            else UpdateBoss03(velocity.magnitude);
        }

        private void UpdateBoss01(float speed)
        {
            var state = boss01.State;
            var pattern = boss01.CurrentPattern;
            var phase = boss01.Phase;
            int segment = (int)state * 100 + (int)pattern * 20 + (int)phase * 4 + boss01.SwingIndex;
            if (pattern == BossController.Pattern.MeleeCombo)
            {
                if (phase == BossController.AttackPhase.Windup || phase == BossController.AttackPhase.Interval)
                    PlayTimed(Melee[Mathf.Clamp(boss01.SwingIndex, 0, 2)], segment, boss01.PhaseTimeRemaining, 0f, 0.32f);
                else if (phase == BossController.AttackPhase.Active)
                    PlayTimed(Melee[Mathf.Clamp(boss01.SwingIndex - 1, 0, 2)], segment, boss01.PhaseTimeRemaining, 0.32f, 0.6f);
                else if (state == BossController.BossState.Recovery)
                    PlayTimed(Melee[2], segment, boss01.PhaseTimeRemaining, 0.6f, 1f);
                else PlayLoop(Locomotion);
            }
            else if (pattern == BossController.Pattern.Charge && state == BossController.BossState.Attack)
                PlayLoop(phase == BossController.AttackPhase.Active ? Rush : RushReady,
                    phase == BossController.AttackPhase.Active ? speed / rushClipSpeed : 1f);
            else if (pattern == BossController.Pattern.CircleAoE)
                PlayTimed(GroundCast, segment, boss01.PhaseTimeRemaining,
                    state == BossController.BossState.Recovery ? 0.5f : 0f,
                    state == BossController.BossState.Recovery ? 1f : 0.5f);
            else PlayLoop(Locomotion);
        }

        private void UpdateBoss02()
        {
            var state = boss02.State;
            if (state == Boss02Controller.BossState.Windup || state == Boss02Controller.BossState.Recovery)
            {
                bool recovering = state == Boss02Controller.BossState.Recovery;
                int segment = (int)state * 10 + (int)boss02.CurrentPattern;
                PlayTimed(boss02.CurrentPattern == Boss02Controller.Pattern.GroundAoE ? GroundCast : Cast,
                    segment, boss02.PhaseTimeRemaining, recovering ? 0.45f : 0f, recovering ? 1f : 0.45f);
            }
            else PlayLoop(Locomotion);
        }

        private void UpdateBoss03(float speed)
        {
            var state = boss03.State;
            var pattern = boss03.CurrentPattern;
            if (pattern == Boss03Controller.Pattern.SequentialGroundBurst)
            {
                // Keep one uninterrupted loop across successive windups and burst intervals.
                PlayLoop(GroundCast);
            }
            else if (pattern == Boss03Controller.Pattern.DashStrike &&
                (state == Boss03Controller.BossState.Windup || state == Boss03Controller.BossState.Dash))
                PlayLoop(state == Boss03Controller.BossState.Dash ? Rush : RushReady,
                    state == Boss03Controller.BossState.Dash ? speed / rushClipSpeed : 1f);
            else if (pattern == Boss03Controller.Pattern.RadialBarrage)
            {
                bool recovering = state == Boss03Controller.BossState.Recovery;
                PlayTimed(Cast, (int)state, boss03.PhaseTimeRemaining,
                    recovering ? 0.45f : 0f, recovering ? 1f : 0.45f);
            }
            else PlayLoop(Locomotion);
        }

        private void PlayTimed(int animation, int segment, float remaining, float from, float to)
        {
            if (currentSegment != segment || currentAnimation != animation)
            {
                currentSegment = segment;
                segmentDuration = Mathf.Max(remaining, 0.0001f);
            }
            // Sample against the controller's timer, including its captured Phase 2 timing.
            // No animation event or duplicated cooldown/phase calculation controls gameplay.
            float progress = 1f - Mathf.Clamp01(remaining / segmentDuration);
            animator.SetFloat(ActionTime, Mathf.Lerp(from, to, progress));
            SetAnimation(animation, 1f);
        }

        private void PlayLoop(int animation, float speed = 1f)
        {
            currentSegment = -1;
            SetAnimation(animation, speed);
        }

        private void SetAnimation(int animation, float speed)
        {
            animator.SetFloat(MotionSpeed, speed);
            if (currentAnimation == animation) return;
            if (animation == Locomotion) animator.CrossFadeInFixedTime(animation, movementBlend, 0);
            else animator.Play(animation, 0, 0f);
            currentAnimation = animation;
        }
    }
}
