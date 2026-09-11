using UnityEngine;

namespace AshenTrial
{
    [CreateAssetMenu(menuName = "Ashen Trial/Boss 03 Config")]
    public sealed class Boss03Config : ScriptableObject
    {
        [Header("General")]
        [Min(1f)] public float maxHealth = 400f;
        [Min(0f)] public float moveSpeed = 3.5f;
        [Min(0f)] public float rotationSpeed = 540f;
        [Min(0f)] public float preferredMinDistance = 3.5f;
        [Min(0.1f)] public float preferredMaxDistance = 6f;
        public Vector2 arenaMin = new Vector2(-25f, -25f);
        public Vector2 arenaMax = new Vector2(25f, 25f);
        public float groundHeight;
        [Min(0f)] public float boundaryMargin = 0.2f;
        [Header("Dash Strike")]
        [Min(0.1f)] public float dashTriggerRange = 9f;
        [Min(0.01f)] public float dashWindup = 0.7f;
        [Min(0.1f)] public float dashSpeed = 16f;
        [Min(0.1f)] public float dashDistance = 6f;
        [Min(0f)] public float dashDamage = 25f;
        [Min(0.01f)] public float dashRecovery = 0.9f;
        [Min(0f)] public float dashCooldown = 3f;
        [Header("Radial Barrage")]
        [Min(0.01f)] public float radialWindup = 0.9f;
        [Min(0.1f)] public float projectileSpeed = 9f;
        [Min(0.1f)] public float projectileLifetime = 2.5f;
        [Min(0f)] public float radialDamage = 18f;
        [Min(0.01f)] public float radialRecovery = 1f;
        [Min(0f)] public float radialCooldown = 4f;
        [Header("Sequential Ground Burst")]
        [Min(0.1f)] public float burstRadius = 2.5f;
        [Min(0f)] public float burstDamage = 22f;
        [Min(0.01f)] public float burstWindup = 0.7f;
        [Min(0.01f)] public float burstInterval = 0.4f;
        [Min(0.01f)] public float burstRecovery = 1f;
        [Min(0f)] public float burstCooldown = 5f;
        [Header("Phase Two")]
        [Range(0.01f, 0.99f)] public float phaseTwoHealthRatio = 0.5f;
        [Min(0.1f)] public float phaseTwoDashSpeed = 18f;
    }
}
