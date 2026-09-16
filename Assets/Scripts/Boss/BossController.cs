using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public sealed class BossController : MonoBehaviour
    {
        public enum BossState { Ready, Chase, Attack, Recovery, Dead }
        public enum AttackPhase { None, Windup, Active, Interval }
        public enum Pattern { None, MeleeCombo, Charge, CircleAoE }
        public enum FightPhase { One = 1, Two = 2 }
        private const float RangeTolerance = 0.01f;
        private const int MeleeSwingCount = 3;

        [SerializeField] private BossConfig config;
        [SerializeField] private Health health;
        [SerializeField] private Transform target;
        [SerializeField] private Health targetHealth;
        [SerializeField] private AttackHitbox hitbox;
        [SerializeField] private BossTelegraph telegraph;
        [SerializeField] private GameAudio gameAudio;
        [SerializeField, Min(0f)] private float meleeDirectionLockLead = 0.15f;
        [SerializeField] private FightPhase fightPhase = FightPhase.One;
        [SerializeField] private BossState state;
        [SerializeField] private AttackPhase phase;
        [SerializeField] private Pattern currentPattern;
        [SerializeField] private Pattern previousPattern;
        [SerializeField] private int swingIndex;
        private CharacterController controller;
        private float remainingTime;
        private float verticalSpeed;
        private Vector3 chargeDirection;
        private float chargeRemaining;
        private float chargeReadyAt;
        private float aoeReadyAt;
        private bool patternPhaseTwo;

        public BossState State => state;
        public float PhaseTimeRemaining => remainingTime;
        public AttackPhase Phase => phase;
        public Pattern CurrentPattern => currentPattern;
        public Pattern PreviousPattern => previousPattern;
        public int SwingIndex => swingIndex;
        public FightPhase CurrentFightPhase => fightPhase;
        public float CurrentAoERadius => state == BossState.Attack || state == BossState.Recovery ?
            (patternPhaseTwo ? config.PhaseTwoAoERadius : config.AoERadius) :
            (fightPhase == FightPhase.Two ? config.PhaseTwoAoERadius : config.AoERadius);
        public float ChargeCooldownRemaining => Mathf.Max(0f, chargeReadyAt - Time.time);
        public float AoECooldownRemaining => Mathf.Max(0f, aoeReadyAt - Time.time);

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (config == null || health == null || target == null || targetHealth == null || hitbox == null || telegraph == null)
            {
                Debug.LogError("BossController: Config, Health, Target, Target Health, Hitbox and Telegraph references are required.", this);
                enabled = false;
                return;
            }
            health.Initialize(config.MaxHealth);
            fightPhase = FightPhase.One;
            // The player's forward marker has a solid collider outside its body.
            // Keep body collision, but do not let visual child colliders block approach.
            BossLocomotion.IgnoreTargetChildSolidColliders(controller, target);
            StopAttack();
            state = BossState.Ready;
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Died += OnDeath;
                health.Damaged += UpdateFightPhase;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDeath;
                health.Damaged -= UpdateFightPhase;
            }
            StopAttack();
            verticalSpeed = 0f;
            state = health != null && health.IsDead ? BossState.Dead : BossState.Ready;
        }

        private void Update()
        {
            if (health.IsDead)
            {
                if (state != BossState.Dead) OnDeath();
                return;
            }
            UpdateFightPhase();
            if (target == null || targetHealth == null || !target.gameObject.activeInHierarchy || targetHealth.IsDead)
            {
                StopAttack();
                state = BossState.Ready;
                return;
            }
            if (!controller.enabled) return;

            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            Vector3 displacement = Vector3.zero;
            if (state == BossState.Ready || state == BossState.Chase)
            {
                RotateTowards(toTarget);
                float distance = toTarget.magnitude;
                Pattern selected = SelectPattern(distance);
                if (selected != Pattern.None)
                {
                    StartPattern(selected);
                }
                else if (distance > config.AttackRange + RangeTolerance)
                {
                    state = BossState.Chase;
                    float step = Mathf.Min(config.MoveSpeed * Time.deltaTime, distance - config.AttackRange);
                    displacement = toTarget.normalized * step;
                }
                else
                {
                    state = BossState.Ready;
                }
            }
            else if (state == BossState.Attack)
            {
                displacement = UpdatePattern(toTarget);
            }
            else if (state == BossState.Recovery)
            {
                remainingTime -= Time.deltaTime;
                if (remainingTime <= 0f)
                {
                    currentPattern = Pattern.None;
                    state = BossState.Ready;
                }
            }

            // A lethal hit can disable this boss synchronously through GameOver.
            if (!isActiveAndEnabled || !controller.enabled) return;
            verticalSpeed = BossLocomotion.TickGravity(controller, verticalSpeed, Time.deltaTime);
            Vector3 previousPosition = transform.position;
            CollisionFlags collision = controller.Move(displacement + Vector3.up * (verticalSpeed * Time.deltaTime));
            if ((collision & CollisionFlags.Above) != 0 && verticalSpeed > 0f) verticalSpeed = 0f;
            if (currentPattern == Pattern.Charge && phase == AttackPhase.Active)
            {
                Vector3 actualMovement = transform.position - previousPosition;
                actualMovement.y = 0f;
                hitbox.SweepMovement(actualMovement);
                if (chargeRemaining <= 0f || (collision & CollisionFlags.Sides) != 0)
                    EnterRecovery();
            }
        }

        private Pattern SelectPattern(float distance)
        {
            bool melee = distance <= config.AttackRange + RangeTolerance;
            bool charge = !melee && distance <= config.ChargeTriggerRange + RangeTolerance &&
                ChargeCooldownRemaining <= 0f;
            bool aoe = distance <= CurrentAoERadius && AoECooldownRemaining <= 0f;
            if (melee && previousPattern != Pattern.MeleeCombo) return Pattern.MeleeCombo;
            if (charge && previousPattern != Pattern.Charge) return Pattern.Charge;
            if (aoe && previousPattern != Pattern.CircleAoE) return Pattern.CircleAoE;
            // Reuse the last pattern only when no other eligible candidate exists.
            if (melee) return Pattern.MeleeCombo;
            if (charge) return Pattern.Charge;
            if (aoe) return Pattern.CircleAoE;
            return Pattern.None;
        }

        private void StartPattern(Pattern pattern)
        {
            currentPattern = pattern;
            previousPattern = pattern;
            // Keep an in-flight pattern's timing and warning radius stable across the HP threshold.
            patternPhaseTwo = fightPhase == FightPhase.Two;
            swingIndex = 0;
            phase = AttackPhase.Windup;
            state = BossState.Attack;
            remainingTime = pattern == Pattern.MeleeCombo ? config.AttackWindup :
                pattern == Pattern.Charge ? config.ChargeWindup : config.AoEWindup;
            telegraph.Show(pattern, config.ChargeDistance, CurrentAoERadius);
        }

        private Vector3 UpdatePattern(Vector3 toTarget)
        {
            if (currentPattern == Pattern.Charge && phase == AttackPhase.Active)
            {
                float speed = patternPhaseTwo ? config.PhaseTwoChargeSpeed : config.ChargeSpeed;
                float step = Mathf.Min(speed * Time.deltaTime, chargeRemaining);
                chargeRemaining -= step;
                return chargeDirection * step;
            }
            if (phase == AttackPhase.Windup && currentPattern != Pattern.CircleAoE &&
                (currentPattern != Pattern.MeleeCombo || remainingTime > Mathf.Min(meleeDirectionLockLead, config.AttackWindup)))
                RotateTowards(toTarget);
            if (phase == AttackPhase.Windup)
                telegraph.Show(currentPattern, config.ChargeDistance, CurrentAoERadius);
            remainingTime -= Time.deltaTime;
            if (remainingTime > 0f) return Vector3.zero;
            telegraph.Hide();

            if (currentPattern == Pattern.MeleeCombo)
            {
                if (phase == AttackPhase.Windup || phase == AttackPhase.Interval)
                {
                    swingIndex++;
                    hitbox.BeginSwing(config.AttackDamage, transform);
                    gameAudio?.PlayBossMeleeSwing();
                    hitbox.SetActive(true);
                    phase = AttackPhase.Active;
                    remainingTime = config.AttackActiveDuration;
                }
                else if (swingIndex < MeleeSwingCount)
                {
                    hitbox.SetActive(false);
                    phase = AttackPhase.Interval;
                    remainingTime = config.MeleeInterval;
                    telegraph.Show(currentPattern, config.ChargeDistance, CurrentAoERadius);
                }
                else EnterRecovery();
            }
            else if (currentPattern == Pattern.Charge)
            {
                chargeDirection = transform.forward;
                chargeDirection.y = 0f;
                chargeDirection.Normalize();
                chargeRemaining = config.ChargeDistance;
                hitbox.BeginSwing(config.ChargeDamage, transform);
                gameAudio?.PlayBossCharge();
                hitbox.SetActive(true);
                phase = AttackPhase.Active;
            }
            else if (currentPattern == Pattern.CircleAoE)
            {
                gameAudio?.PlayBossAoE();
                if (toTarget.magnitude <= CurrentAoERadius)
                    targetHealth.TakeDamage(config.AoEDamage);
                EnterRecovery();
            }
            return Vector3.zero;
        }

        private void EnterRecovery()
        {
            telegraph.Hide();
            hitbox.SetActive(false);
            phase = AttackPhase.None;
            state = BossState.Recovery;
            if (currentPattern == Pattern.Charge)
            {
                remainingTime = config.ChargeRecovery;
                chargeReadyAt = Time.time + config.ChargeCooldown;
            }
            else if (currentPattern == Pattern.CircleAoE)
            {
                remainingTime = config.AoERecovery;
                aoeReadyAt = Time.time + (patternPhaseTwo ? config.PhaseTwoAoECooldown : config.AoECooldown);
            }
            else remainingTime = patternPhaseTwo ? config.PhaseTwoMeleeRecovery : config.AttackRecovery;
        }

        private void UpdateFightPhase()
        {
            if (health.IsDead || fightPhase == FightPhase.Two) return;
            if (health.CurrentHp / health.MaxHp <= config.PhaseTwoHealthRatio)
                fightPhase = FightPhase.Two;
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return;
            Quaternion desired = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, config.RotationSpeed * Time.deltaTime);
        }

        private void StopAttack()
        {
            if (telegraph != null) telegraph.Hide();
            if (hitbox != null) hitbox.SetActive(false);
            phase = AttackPhase.None;
            currentPattern = Pattern.None;
            remainingTime = 0f;
            swingIndex = 0;
            chargeRemaining = 0f;
            chargeDirection = Vector3.zero;
        }

        private void OnDeath()
        {
            StopAttack();
            verticalSpeed = 0f;
            state = BossState.Dead;
        }

        private void OnDrawGizmosSelected()
        {
            if (config == null) return;
            Color previous = Gizmos.color;
            Gizmos.color = Color.cyan;
            const int segments = 64;
            float radius = Application.isPlaying ? CurrentAoERadius : config.AoERadius;
            Vector3 previousPoint = transform.position + Vector3.right * radius;
            for (int index = 1; index <= segments; index++)
            {
                float angle = index * Mathf.PI * 2f / segments;
                Vector3 point = transform.position +
                    new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
            Gizmos.color = previous;
        }
    }
}
