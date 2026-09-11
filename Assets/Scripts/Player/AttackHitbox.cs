using System.Collections.Generic;
using UnityEngine;

namespace AshenTrial
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public sealed class AttackHitbox : MonoBehaviour
    {
        [SerializeField, Tooltip("Optional target restriction. Empty allows any Health except the owner.")]
        private Health targetHealth;
        private readonly HashSet<Health> hitTargets = new HashSet<Health>();
        private BoxCollider hitCollider;
        private Transform owner;
        private float damage;
        public bool IsActive { get; private set; }

        private void Awake()
        {
            hitCollider = GetComponent<BoxCollider>();
            SetActive(false);
        }

        public void BeginSwing(float amount, Transform attacker)
        {
            SetActive(false);
            hitTargets.Clear();
            damage = amount;
            owner = attacker;
        }

        public void SetActive(bool active)
        {
            if (hitCollider == null) hitCollider = GetComponent<BoxCollider>();
            IsActive = active;
            hitCollider.enabled = active;
            if (!active) return;

            // Catch targets already inside when the collider is enabled, without per-frame searches.
            Physics.SyncTransforms();
            Vector3 scale = transform.lossyScale;
            Vector3 halfSize = Vector3.Scale(hitCollider.size * 0.5f,
                new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            foreach (Collider other in Physics.OverlapBox(transform.TransformPoint(hitCollider.center),
                halfSize, transform.rotation, Physics.AllLayers, QueryTriggerInteraction.Collide))
                TryHit(other);
        }

        private void OnTriggerEnter(Collider other) => TryHit(other);

        public void SweepMovement(Vector3 displacement)
        {
            if (!IsActive || displacement.sqrMagnitude < 0.000001f) return;
            Vector3 scale = transform.lossyScale;
            Vector3 halfSize = Vector3.Scale(hitCollider.size * 0.5f,
                new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            Vector3 previousCenter = transform.TransformPoint(hitCollider.center) - displacement;
            foreach (RaycastHit hit in Physics.BoxCastAll(previousCenter, halfSize,
                displacement.normalized, transform.rotation, displacement.magnitude,
                Physics.AllLayers, QueryTriggerInteraction.Collide))
                TryHit(hit.collider);
        }

        private void TryHit(Collider other)
        {
            if (!IsActive || owner == null || other.transform.IsChildOf(owner)) return;
            Health health = other.GetComponentInParent<Health>();
            if (targetHealth != null && health != targetHealth) return;
            if (health == null || !health.isActiveAndEnabled || health.IsDead ||
                health.transform.IsChildOf(owner)) return;
            // Child attack triggers and visual markers must not extend a character's hurt volume.
            if (health.TryGetComponent<CharacterController>(out var body) && other != body) return;
            if (!hitTargets.Add(health)) return;
            health.TakeDamage(damage);
        }

        private void OnDisable() => SetActive(false);

        private void OnDrawGizmos()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            if (box == null) return;
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Color previousColor = Gizmos.color;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = IsActive ? Color.red : Color.yellow;
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }
    }
}
