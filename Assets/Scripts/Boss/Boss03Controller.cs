using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public sealed class Boss03Controller : MonoBehaviour
    {
        public enum BossState { Decision, Approach, Windup, Dash, BurstInterval, Recovery, Dead }
        public enum Pattern { None, DashStrike, RadialBarrage, SequentialGroundBurst }
        [SerializeField] private Boss03Config config;
        [SerializeField] private Health health;
        [SerializeField] private Transform target;
        [SerializeField] private Health targetHealth;
        [SerializeField] private CharacterController targetBody;
        [SerializeField] private AttackHitbox dashHitbox;
        [SerializeField] private BossProjectilePool projectilePool;
        [SerializeField] private Transform projectileSpawn;
        [SerializeField] private Boss03Telegraph telegraph;
        [SerializeField] private BossState state;
        [SerializeField] private Pattern currentPattern;
        [SerializeField] private Pattern previousPattern;
        [SerializeField] private bool phaseTwo;
        [SerializeField] private int burstIndex;
        private CharacterController body;
        private readonly float[] readyAt = new float[4];
        private bool patternPhaseTwo;
        private float remainingTime;
        private float dashRemaining;
        private float verticalSpeed;
        private float projectileWidth;
        private Vector3 dashDirection;
        private Vector3 burstTarget;

        public BossState State => state;
        public float PhaseTimeRemaining => remainingTime;
        public Pattern CurrentPattern => currentPattern;
        public bool PhaseTwo => phaseTwo;
        public int BurstIndex => burstIndex;
        public Vector3 BurstTarget => burstTarget;
        public int CurrentBurstCount => patternPhaseTwo ? 4 : 3;
        public int CurrentRadialCount => patternPhaseTwo ? 12 : 8;
        public float CurrentDashSpeed => patternPhaseTwo ? config.phaseTwoDashSpeed : config.dashSpeed;

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            if (config == null || health == null || target == null || targetHealth == null || targetBody == null ||
                dashHitbox == null || projectilePool == null || projectileSpawn == null || telegraph == null)
            {
                Debug.LogError("Boss03Controller: Config, Health, player, hitbox, pool, spawn and telegraph references are required.", this);
                enabled = false;
                return;
            }
            health.Initialize(config.maxHealth);
            projectileWidth = projectilePool.ProjectileWidth;
            foreach (Collider other in target.GetComponentsInChildren<Collider>())
                if (other.transform != target && !other.isTrigger) Physics.IgnoreCollision(body, other);
            CancelPattern();
        }

        private void OnEnable()
        {
            if (health == null) return;
            health.Died += OnDeath;
            health.Damaged += UpdatePhase;
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Died -= OnDeath;
                health.Damaged -= UpdatePhase;
            }
            CancelPattern();
            verticalSpeed = 0f;
            if (projectilePool != null) projectilePool.ReleaseAllActive();
            state = health != null && health.IsDead ? BossState.Dead : BossState.Decision;
        }

        private void UpdatePhase()
        {
            if (!health.IsDead && !phaseTwo && health.CurrentHp <= health.MaxHp * config.phaseTwoHealthRatio)
                phaseTwo = true;
        }

        private void Update()
        {
            if (health.IsDead) { OnDeath(); return; }
            if (target == null || targetHealth == null || targetHealth.IsDead || !target.gameObject.activeInHierarchy)
            {
                CancelPattern();
                state = BossState.Decision;
                return;
            }
            if (!body.enabled) return;
            UpdatePhase();
            Vector3 toTarget = target.position - transform.position;
            toTarget.y = 0f;
            if (state == BossState.Decision || state == BossState.Approach ||
                (state == BossState.Windup && currentPattern != Pattern.SequentialGroundBurst))
            {
                if (toTarget.sqrMagnitude > 0.000001f)
                    transform.rotation = Quaternion.RotateTowards(transform.rotation,
                        Quaternion.LookRotation(toTarget), config.rotationSpeed * Time.deltaTime);
            }
            Vector3 movement = Vector3.zero;
            switch (state)
            {
                case BossState.Decision:
                case BossState.Approach:
                    Pattern selected = SelectPattern(toTarget.magnitude);
                    if (selected != Pattern.None) StartPattern(selected);
                    else if (toTarget.magnitude > config.preferredMaxDistance)
                    {
                        state = BossState.Approach;
                        movement = toTarget.normalized * Mathf.Min(config.moveSpeed * Time.deltaTime,
                            toTarget.magnitude - config.preferredMaxDistance);
                    }
                    else state = BossState.Decision;
                    break;
                case BossState.Windup:
                    ShowTelegraph();
                    remainingTime -= Time.deltaTime;
                    if (remainingTime <= 0f) ExecuteWindup();
                    break;
                case BossState.Dash:
                    float step = Mathf.Min(CurrentDashSpeed * Time.deltaTime, dashRemaining);
                    dashRemaining -= step;
                    movement = dashDirection * step;
                    break;
                case BossState.BurstInterval:
                    remainingTime -= Time.deltaTime;
                    if (remainingTime <= 0f) StartBurst();
                    break;
                case BossState.Recovery:
                    remainingTime -= Time.deltaTime;
                    if (remainingTime <= 0f) { currentPattern = Pattern.None; state = BossState.Decision; }
                    break;
            }
            if (!isActiveAndEnabled || !body.enabled) return;
            float margin = body.radius + body.skinWidth + config.boundaryMargin;
            Vector3 destination = transform.position + movement;
            destination.x = Mathf.Clamp(destination.x, config.arenaMin.x + margin, config.arenaMax.x - margin);
            destination.z = Mathf.Clamp(destination.z, config.arenaMin.y + margin, config.arenaMax.y - margin);
            Vector3 boundedMovement = destination - transform.position;
            bool reachedBoundary = (boundedMovement - movement).sqrMagnitude > 0.000001f;
            if (body.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
            Vector3 previousPosition = transform.position;
            CollisionFlags collision = body.Move(boundedMovement + Vector3.up * (verticalSpeed * Time.deltaTime));
            if (state == BossState.Dash)
            {
                Vector3 actual = transform.position - previousPosition;
                actual.y = 0f;
                dashHitbox.SweepMovement(actual);
                if (dashRemaining <= 0f || reachedBoundary || (collision & CollisionFlags.Sides) != 0)
                    EnterRecovery();
            }
        }

        private Pattern SelectPattern(float distance)
        {
            if (distance > config.preferredMaxDistance)
                return distance <= config.dashTriggerRange && Time.time >= readyAt[(int)Pattern.DashStrike] ? Pattern.DashStrike : Pattern.None;
            for (int offset = 0; offset < 3; offset++)
            {
                Pattern candidate = (Pattern)(((int)previousPattern + offset) % 3 + 1);
                if (candidate == Pattern.DashStrike && distance < config.preferredMinDistance) continue;
                if (Time.time >= readyAt[(int)candidate]) return candidate;
            }
            return Pattern.None;
        }

        private void StartPattern(Pattern pattern)
        {
            currentPattern = pattern;
            previousPattern = pattern;
            patternPhaseTwo = phaseTwo;
            burstIndex = 0;
            telegraph.Hide();
            if (pattern == Pattern.SequentialGroundBurst) { StartBurst(); return; }
            state = BossState.Windup;
            remainingTime = pattern == Pattern.DashStrike ? config.dashWindup : config.radialWindup;
            ShowTelegraph();
        }

        private void StartBurst()
        {
            burstIndex++;
            burstTarget = target.position;
            burstTarget.y = config.groundHeight;
            state = BossState.Windup;
            remainingTime = config.burstWindup;
            ShowTelegraph();
        }

        private void ShowTelegraph()
        {
            if (currentPattern == Pattern.DashStrike) telegraph.ShowDash(config.dashDistance, config.groundHeight);
            else if (currentPattern == Pattern.RadialBarrage)
                telegraph.ShowRadial(projectileSpawn.position, transform.forward, CurrentRadialCount,
                    config.projectileSpeed * config.projectileLifetime, projectileWidth, config.groundHeight);
            else telegraph.ShowBurst(burstTarget, config.burstRadius);
        }

        private void ExecuteWindup()
        {
            telegraph.Hide();
            if (currentPattern == Pattern.DashStrike)
            {
                dashDirection = transform.forward;
                dashRemaining = config.dashDistance;
                state = BossState.Dash;
                dashHitbox.BeginSwing(config.dashDamage, transform);
                dashHitbox.SetActive(true);
            }
            else if (currentPattern == Pattern.RadialBarrage)
            {
                for (int i = 0; i < CurrentRadialCount; i++)
                {
                    Vector3 direction = Quaternion.Euler(0f, i * (360f / CurrentRadialCount), 0f) * transform.forward;
                    projectilePool.Spawn(projectileSpawn.position, direction, config.projectileSpeed,
                        config.radialDamage, config.projectileLifetime, targetHealth, targetBody);
                }
                EnterRecovery();
            }
            else
            {
                Vector3 delta = target.position - burstTarget;
                delta.y = 0f;
                if (delta.sqrMagnitude <= config.burstRadius * config.burstRadius) targetHealth.TakeDamage(config.burstDamage);
                if (burstIndex < CurrentBurstCount)
                {
                    state = BossState.BurstInterval;
                    remainingTime = config.burstInterval;
                }
                else EnterRecovery();
            }
        }

        private void EnterRecovery()
        {
            telegraph.Hide();
            dashHitbox.SetActive(false);
            state = BossState.Recovery;
            remainingTime = currentPattern == Pattern.DashStrike ? config.dashRecovery :
                currentPattern == Pattern.RadialBarrage ? config.radialRecovery : config.burstRecovery;
            readyAt[(int)currentPattern] = Time.time + (currentPattern == Pattern.DashStrike ? config.dashCooldown :
                currentPattern == Pattern.RadialBarrage ? config.radialCooldown : config.burstCooldown);
        }

        private void CancelPattern()
        {
            if (telegraph != null) telegraph.Hide();
            if (dashHitbox != null) dashHitbox.SetActive(false);
            currentPattern = Pattern.None;
            remainingTime = 0f;
            dashRemaining = 0f;
            burstIndex = 0;
        }

        private void OnDeath()
        {
            CancelPattern();
            if (projectilePool != null) projectilePool.ReleaseAllActive();
            verticalSpeed = 0f;
            state = BossState.Dead;
        }
    }
}
