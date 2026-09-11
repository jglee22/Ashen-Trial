using UnityEngine;
using UnityEngine.Pool;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public sealed class BossProjectile : MonoBehaviour
    {
        private SphereCollider hitCollider;
        private CharacterController targetBody;
        private Health targetHealth;
        private float damage;
        private float remainingLifetime;
        private bool launched;
        private bool spent;
        private IObjectPool<BossProjectile> pool;

        public Vector3 Direction { get; private set; }
        public float Speed { get; private set; }

        private void Awake() => hitCollider = GetComponent<SphereCollider>();

        public void Launch(Vector3 position, Vector3 direction, float speed, float amount, float lifetime,
            Health target, CharacterController body, IObjectPool<BossProjectile> ownerPool)
        {
            pool = ownerPool;
            launched = true;
            if (target == null || body == null || direction.sqrMagnitude < 0.000001f || speed <= 0f || lifetime <= 0f)
            {
                Debug.LogError("BossProjectile: Valid direction, speed, lifetime and target references are required.", this);
                Release();
                return;
            }
            Direction = direction.normalized;
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(Direction));
            Speed = speed;
            damage = amount;
            remainingLifetime = lifetime;
            targetHealth = target;
            targetBody = body;
            spent = false;
            hitCollider.enabled = true;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!launched || spent) return;
            float step = Mathf.Min(Time.deltaTime, remainingLifetime);
            Vector3 displacement = Direction * (Speed * step);
            // Sweep only against the designated body, ignoring visual and attack colliders.
            // This also catches a fast projectile crossing the entire body between frames.
            if (targetBody != null && targetBody.enabled && targetBody.gameObject.activeInHierarchy)
            {
                float radius = hitCollider.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x),
                    Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z)));
                Vector3 start = transform.TransformPoint(hitCollider.center);
                Vector3 scale = targetBody.transform.lossyScale;
                float bodyRadius = targetBody.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
                float halfSegment = Mathf.Max(0f, targetBody.height * Mathf.Abs(scale.y) * 0.5f - bodyRadius);
                Vector3 center = targetBody.transform.TransformPoint(targetBody.center);
                // Both combatants remain upright on this arena. Closest horizontal point on the
                // travelled segment gives the capsule distance at the projectile's fixed height.
                float along = Mathf.Clamp(Vector3.Dot(center - start, Direction), 0f, displacement.magnitude);
                Vector3 closest = start + Direction * along;
                Vector3 onBody = new Vector3(center.x, Mathf.Clamp(closest.y,
                    center.y - halfSegment, center.y + halfSegment), center.z);
                if ((closest - onBody).sqrMagnitude <= (radius + bodyRadius) * (radius + bodyRadius))
                {
                    Hit();
                    return;
                }
            }
            transform.position += displacement;
            remainingLifetime -= step;
            if (remainingLifetime <= 0f) Release();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (launched && !spent && other == targetBody) Hit();
        }

        private void Hit()
        {
            spent = true;
            hitCollider.enabled = false;
            if (targetHealth != null && targetHealth.isActiveAndEnabled && !targetHealth.IsDead)
                targetHealth.TakeDamage(damage);
            Release();
        }

        internal void Release()
        {
            if (!launched) return;
            launched = false;
            spent = true;
            hitCollider.enabled = false;
            targetHealth = null;
            targetBody = null;
            Direction = Vector3.zero;
            Speed = 0f;
            damage = 0f;
            remainingLifetime = 0f;
            gameObject.SetActive(false);
            IObjectPool<BossProjectile> ownerPool = pool;
            pool = null;
            ownerPool.Release(this);
        }
    }
}
