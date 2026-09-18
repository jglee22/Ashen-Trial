using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    public sealed class Health : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maxHp = 100f;
        [SerializeField, Tooltip("Runtime HP; initialized from Max HP in Awake.")]
        private float currentHp;
        [SerializeField] private bool deactivateOnDeath = true;

        public System.Func<bool> DamageBlocked { private get; set; }
        public event System.Action Damaged;
        public event System.Action<float> DamageApplied;
        public event System.Action Died;
        public event System.Action Changed;

        public float MaxHp => Mathf.Max(1f, maxHp);
        public float CurrentHp => currentHp;
        public bool IsDead => currentHp <= 0f;

        public bool TryGetLastHitPoint(out Vector3 hitPoint)
        {
            hitPoint = lastHitPoint;
            return hasLastHitPoint;
        }

        private void Awake()
        {
            if (!initialized) Initialize(MaxHp);
        }

        private bool initialized;
        private Vector3 lastHitPoint;
        private bool hasLastHitPoint;

        public void Initialize(float maximum)
        {
            maxHp = Mathf.Max(1f, maximum);
            currentHp = MaxHp;
            initialized = true;
            Changed?.Invoke();
        }

        public void IncreaseMaxHealth(float amount)
        {
            if (amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount)) return;
            bool wasDead = IsDead;
            maxHp = MaxHp + amount;
            if (!wasDead) currentHp = Mathf.Min(MaxHp, currentHp + amount);
            Changed?.Invoke();
        }

        public void TakeDamage(float amount) => TakeDamage(amount, false, default);

        public void TakeDamage(float amount, Vector3 hitPoint) => TakeDamage(amount, true, hitPoint);

        private void TakeDamage(float amount, bool hasHitPoint, Vector3 hitPoint)
        {
            if (!isActiveAndEnabled || IsDead || amount <= 0f || float.IsNaN(amount)) return;
            if (DamageBlocked != null && DamageBlocked()) return;
            float previousHp = currentHp;
            currentHp = Mathf.Max(0f, currentHp - amount);
            float appliedDamage = previousHp - currentHp;
            if (appliedDamage > 0f) Changed?.Invoke();
            Damaged?.Invoke();
            if (appliedDamage > 0f)
            {
                hasLastHitPoint = hasHitPoint;
                lastHitPoint = hitPoint;
                DamageApplied?.Invoke(appliedDamage);
                hasLastHitPoint = false;
            }
            if (IsDead)
            {
                Died?.Invoke();
                if (deactivateOnDeath) gameObject.SetActive(false);
            }
        }
    }
}
