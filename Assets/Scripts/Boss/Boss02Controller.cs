using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController), typeof(Health))]
    public sealed class Boss02Controller : MonoBehaviour
    {
        public enum BossState { Decision, Approach, Reposition, Windup, Recovery, Dead }
        public enum Pattern { None, StraightShot, FanShot, GroundAoE }
        [SerializeField] private Boss02Config config;
        [SerializeField] private Health health;
        [SerializeField] private Transform target;
        [SerializeField] private Health targetHealth;
        [SerializeField] private CharacterController targetBody;
        [SerializeField] private BossProjectilePool projectilePool;
        [SerializeField] private Transform projectileSpawn;
        [SerializeField] private Boss02Telegraph telegraph;
        [SerializeField] private BossState state;
        [SerializeField] private Pattern currentPattern;
        [SerializeField] private Pattern previousPattern;
        [SerializeField] private bool phaseTwo;
        private CharacterController body;
        private readonly float[] readyAt = new float[4];
        private float repositionReadyAt;
        private float remainingTime;
        private float verticalSpeed;
        private float projectileWidth;
        private bool patternPhaseTwo;
        private Vector3 repositionTarget;
        private Vector3 groundTarget;

        public BossState State => state;
        public float PhaseTimeRemaining => remainingTime;
        public Pattern CurrentPattern => currentPattern;
        public bool PhaseTwo => phaseTwo;
        public Vector3 GroundTarget => groundTarget;
        public float CurrentAoERadius => patternPhaseTwo ? config.phaseTwoAoERadius : config.aoeRadius;
        public int CurrentShotCount => currentPattern == Pattern.FanShot ? (patternPhaseTwo ? 5 : 3) : 1;
        public float CurrentShotSpeed => currentPattern == Pattern.StraightShot && patternPhaseTwo ?
            config.phaseTwoStraightSpeed : config.projectileSpeed;

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            if (config == null || health == null || target == null || targetHealth == null || targetBody == null ||
                projectilePool == null || projectileSpawn == null || telegraph == null)
            {
                Debug.LogError("Boss02Controller: Config, Health, player, projectile and telegraph references are required.", this);
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
            if (!phaseTwo && health.CurrentHp <= health.MaxHp * config.phaseTwoHealthRatio) phaseTwo = true;
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
            if (toTarget.sqrMagnitude > 0.000001f)
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(toTarget), config.turnSpeed * Time.deltaTime);
            Vector3 movement = Vector3.zero;
            switch (state)
            {
                case BossState.Decision:
                case BossState.Approach:
                    float distance = toTarget.magnitude;
                    if (distance < config.preferredMinDistance && Time.time >= repositionReadyAt)
                        StartReposition(toTarget);
                    else if (distance > config.preferredMaxDistance + 0.01f)
                    {
                        state = BossState.Approach;
                        movement = toTarget.normalized * Mathf.Min(config.moveSpeed * Time.deltaTime,
                            distance - config.preferredMaxDistance);
                    }
                    else
                    {
                        state = BossState.Decision;
                        Pattern selected = SelectPattern();
                        if (selected != Pattern.None) StartPattern(selected);
                    }
                    break;
                case BossState.Reposition:
                    Vector3 remaining = repositionTarget - transform.position;
                    remaining.y = 0f;
                    movement = Vector3.ClampMagnitude(remaining, config.repositionSpeed * Time.deltaTime);
                    remainingTime -= Time.deltaTime;
                    if (remaining.magnitude <= 0.05f || remainingTime <= 0f) state = BossState.Decision;
                    break;
                case BossState.Windup:
                    if (currentPattern != Pattern.GroundAoE) ShowShotTelegraph();
                    else telegraph.ShowCircle(groundTarget, CurrentAoERadius);
                    remainingTime -= Time.deltaTime;
                    if (remainingTime <= 0f) FirePattern();
                    break;
                case BossState.Recovery:
                    remainingTime -= Time.deltaTime;
                    if (remainingTime <= 0f) { currentPattern = Pattern.None; state = BossState.Decision; }
                    break;
            }
            if (!isActiveAndEnabled || !body.enabled) return;
            Vector3 bounded = ClampToArena(transform.position + movement);
            movement.x = bounded.x - transform.position.x;
            movement.z = bounded.z - transform.position.z;
            if (body.isGrounded && verticalSpeed < 0f) verticalSpeed = -2f;
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
            body.Move(movement + Vector3.up * (verticalSpeed * Time.deltaTime));
        }

        private Vector3 ClampToArena(Vector3 position)
        {
            float margin = body.radius + body.skinWidth + config.boundaryMargin;
            position.x = Mathf.Clamp(position.x, config.arenaMin.x + margin, config.arenaMax.x - margin);
            position.z = Mathf.Clamp(position.z, config.arenaMin.y + margin, config.arenaMax.y - margin);
            return position;
        }

        private void StartReposition(Vector3 toTarget)
        {
            Vector3 away = toTarget.sqrMagnitude > 0.000001f ? -toTarget.normalized : -transform.forward;
            repositionTarget = ClampToArena(transform.position + away * config.repositionDistance);
            if ((repositionTarget - transform.position).sqrMagnitude < config.repositionDistance * config.repositionDistance * 0.25f)
            {
                Vector3 side = Vector3.Cross(Vector3.up, away);
                Vector3 left = ClampToArena(transform.position + side * config.repositionDistance);
                Vector3 right = ClampToArena(transform.position - side * config.repositionDistance);
                repositionTarget = (left - target.position).sqrMagnitude >= (right - target.position).sqrMagnitude ? left : right;
            }
            remainingTime = config.repositionDistance / config.repositionSpeed + 0.15f;
            repositionReadyAt = Time.time + remainingTime + config.repositionCooldown;
            currentPattern = Pattern.None;
            state = BossState.Reposition;
        }

        private Pattern SelectPattern()
        {
            for (int offset = 0; offset < 3; offset++)
            {
                Pattern candidate = (Pattern)(((int)previousPattern + offset) % 3 + 1);
                if (Time.time >= readyAt[(int)candidate]) return candidate;
            }
            return Pattern.None;
        }

        private void StartPattern(Pattern pattern)
        {
            currentPattern = pattern;
            previousPattern = pattern;
            patternPhaseTwo = phaseTwo;
            state = BossState.Windup;
            remainingTime = pattern == Pattern.GroundAoE ? config.aoeWindup : config.shotWindup;
            if (pattern == Pattern.GroundAoE)
            {
                groundTarget = target.position;
                groundTarget.y = config.groundHeight;
                telegraph.ShowCircle(groundTarget, CurrentAoERadius);
            }
            else ShowShotTelegraph();
        }

        private void ShowShotTelegraph()
        {
            telegraph.ShowShots(projectileSpawn.position, transform.forward, CurrentShotCount,
                config.fanAngleStep, CurrentShotSpeed * config.projectileLifetime, projectileWidth, config.groundHeight);
        }

        private void FirePattern()
        {
            telegraph.Hide();
            if (currentPattern == Pattern.GroundAoE)
            {
                Vector3 delta = target.position - groundTarget;
                delta.y = 0f;
                if (delta.sqrMagnitude <= CurrentAoERadius * CurrentAoERadius) targetHealth.TakeDamage(config.aoeDamage);
            }
            else
            {
                int count = CurrentShotCount;
                for (int i = 0; i < count; i++)
                {
                    Vector3 direction = Quaternion.Euler(0f, (i - (count - 1) * 0.5f) * config.fanAngleStep, 0f) * transform.forward;
                    projectilePool.Spawn(projectileSpawn.position, direction, CurrentShotSpeed,
                        currentPattern == Pattern.StraightShot ? config.straightDamage : config.fanDamage,
                        config.projectileLifetime, targetHealth, targetBody);
                }
            }
            readyAt[(int)currentPattern] = Time.time + (currentPattern == Pattern.StraightShot ? config.straightCooldown :
                currentPattern == Pattern.FanShot ? config.fanCooldown : config.aoeCooldown);
            remainingTime = config.recovery;
            state = BossState.Recovery;
        }

        private void CancelPattern()
        {
            if (telegraph != null) telegraph.Hide();
            currentPattern = Pattern.None;
            remainingTime = 0f;
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
