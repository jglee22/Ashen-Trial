using UnityEngine;

namespace AshenTrial
{
    [DefaultExecutionOrder(150)]
    [DisallowMultipleComponent]
    public sealed class PlayerAttackSwipeVfx : MonoBehaviour
    {
        [System.Serializable]
        private struct SwipeProfile
        {
            [Range(0f, 1f)] public float startNormalized;
            [Range(0f, 1f)] public float endNormalized;
            [Min(0.01f)] public float trailTime;
            [Min(0.01f)] public float width;
        }

        [SerializeField] private PlayerCombat combat;
        [SerializeField] private PlayerDodge dodge;
        [SerializeField] private Health health;
        [SerializeField] private Animator animator;
        [SerializeField] private TrailRenderer rightHandTrail;
        [SerializeField] private TrailRenderer leftHandTrail;
        [SerializeField] private TrailRenderer rightFootTrail;
        [SerializeField] private SwipeProfile straight = new SwipeProfile
        {
            startNormalized = 0.14f, endNormalized = 0.32f, trailTime = 0.16f, width = 0.13f
        };
        [SerializeField] private SwipeProfile hook = new SwipeProfile
        {
            startNormalized = 0.22f, endNormalized = 0.54f, trailTime = 0.18f, width = 0.18f
        };
        [SerializeField] private SwipeProfile uppercut = new SwipeProfile
        {
            startNormalized = 0.28f, endNormalized = 0.56f, trailTime = 0.20f, width = 0.20f
        };
        [SerializeField] private SwipeProfile heavy = new SwipeProfile
        {
            startNormalized = 0.16f, endNormalized = 0.56f, trailTime = 0.22f, width = 0.22f
        };

        private static readonly int Attack1 = Animator.StringToHash("Base Layer.Attack1");
        private static readonly int Attack2 = Animator.StringToHash("Base Layer.Attack2");
        private static readonly int Attack3 = Animator.StringToHash("Base Layer.Attack3");
        private static readonly int HeavyAttack = Animator.StringToHash("Base Layer.HeavyAttack");

        private enum SwipeAttack { None, Straight, Hook, Uppercut, Heavy }

        private TrailRenderer[] trails;
        private SwipeAttack activeAttack;
        private bool suppressHit;

        private void Awake()
        {
            if (combat == null || dodge == null || health == null || animator == null ||
                rightHandTrail == null || leftHandTrail == null || rightFootTrail == null)
            {
                Debug.LogError("PlayerAttackSwipeVfx: Combat, dodge, health, animator and three trails are required.", this);
                enabled = false;
                return;
            }
            trails = new[] { rightHandTrail, leftHandTrail, rightFootTrail };
            StopAll(true);
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.DamageApplied += OnDamageApplied;
                health.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.DamageApplied -= OnDamageApplied;
                health.Died -= OnDied;
            }
            StopAll(true);
            suppressHit = false;
            activeAttack = SwipeAttack.None;
        }

        private void OnDied() => StopAll(true);

        private void OnDamageApplied(float amount)
        {
            if (amount <= 0f || health.IsDead) return;
            StopAll(true);
            suppressHit = true;
        }

        private void LateUpdate()
        {
            SwipeAttack attack = ResolveAttack();
            if (attack == SwipeAttack.None)
            {
                suppressHit = false;
                StopAll(attack != activeAttack);
                activeAttack = SwipeAttack.None;
                return;
            }

            if (suppressHit)
            {
                if (attack == activeAttack || activeAttack == SwipeAttack.None)
                {
                    StopAll(false);
                    activeAttack = attack;
                    return;
                }
                suppressHit = false;
            }

            if (attack != activeAttack)
            {
                StopAll(true);
                activeAttack = attack;
            }

            TrailRenderer trail = TrailFor(attack);
            SwipeProfile profile = ProfileFor(attack);
            bool inWindow = IsInSwipeWindow(attack, profile);
            if (inWindow)
            {
                if (!trail.emitting)
                {
                    ApplyProfile(trail, profile);
                    trail.Clear();
                    trail.emitting = true;
                }
            }
            else if (trail.emitting)
            {
                trail.emitting = false;
            }
        }

        private SwipeAttack ResolveAttack()
        {
            if (!isActiveAndEnabled || combat == null || !combat.isActiveAndEnabled) return SwipeAttack.None;
            if (health != null && health.IsDead) return SwipeAttack.None;
            if (dodge != null && dodge.IsDodging) return SwipeAttack.None;
            if (combat.IsHeavyAttacking) return SwipeAttack.Heavy;
            switch (combat.ComboStep)
            {
                case 1: return SwipeAttack.Straight;
                case 2: return SwipeAttack.Hook;
                case 3: return SwipeAttack.Uppercut;
                default: return SwipeAttack.None;
            }
        }

        private bool IsInSwipeWindow(SwipeAttack attack, SwipeProfile profile)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            int hash = StateHash(attack);
            if (state.fullPathHash != hash) return false;
            float normalized = Mathf.Repeat(state.normalizedTime, 1f);
            float start = Mathf.Min(profile.startNormalized, profile.endNormalized);
            float end = Mathf.Max(profile.startNormalized, profile.endNormalized);
            return normalized >= start && normalized <= end;
        }

        private TrailRenderer TrailFor(SwipeAttack attack)
        {
            if (attack == SwipeAttack.Hook) return leftHandTrail;
            if (attack == SwipeAttack.Heavy) return rightFootTrail;
            return rightHandTrail;
        }

        private SwipeProfile ProfileFor(SwipeAttack attack)
        {
            switch (attack)
            {
                case SwipeAttack.Hook: return hook;
                case SwipeAttack.Uppercut: return uppercut;
                case SwipeAttack.Heavy: return heavy;
                default: return straight;
            }
        }

        private static int StateHash(SwipeAttack attack)
        {
            switch (attack)
            {
                case SwipeAttack.Hook: return Attack2;
                case SwipeAttack.Uppercut: return Attack3;
                case SwipeAttack.Heavy: return HeavyAttack;
                default: return Attack1;
            }
        }

        private static void ApplyProfile(TrailRenderer trail, SwipeProfile profile)
        {
            trail.time = profile.trailTime;
            trail.widthMultiplier = profile.width;
        }

        private void StopAll(bool clear)
        {
            if (trails == null) return;
            for (int i = 0; i < trails.Length; i++)
            {
                TrailRenderer trail = trails[i];
                if (trail == null) continue;
                trail.emitting = false;
                if (clear) trail.Clear();
            }
        }
    }
}
