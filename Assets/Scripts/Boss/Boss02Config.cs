using UnityEngine;

namespace AshenTrial
{
    [CreateAssetMenu(menuName = "Ashen Trial/Boss 02 Config")]
    public sealed class Boss02Config : ScriptableObject
    {
        [Header("Health and spacing")]
        [Min(1f)] public float maxHealth = 300f;
        [Min(0f)] public float preferredMinDistance = 5f;
        [Min(0f)] public float preferredMaxDistance = 8f;
        [Min(0f)] public float moveSpeed = 3f;
        [Min(0f)] public float turnSpeed = 720f;
        public Vector2 arenaMin = new Vector2(-25f, -25f);
        public Vector2 arenaMax = new Vector2(25f, 25f);
        public float groundHeight;
        [Min(0f)] public float boundaryMargin = 0.2f;
        [Header("Reposition")]
        [Min(0.1f)] public float repositionDistance = 3f;
        [Min(0.1f)] public float repositionSpeed = 6f;
        [Min(0.1f)] public float repositionCooldown = 3f;
        [Header("Shots")]
        [Min(0.01f)] public float shotWindup = 0.8f;
        [Min(0.01f)] public float recovery = 0.9f;
        [Min(0.1f)] public float projectileSpeed = 10f;
        [Min(0.1f)] public float projectileLifetime = 2.5f;
        [Min(0f)] public float straightDamage = 22f;
        [Min(0f)] public float fanDamage = 18f;
        [Range(1f, 45f)] public float fanAngleStep = 15f;
        [Min(0f)] public float straightCooldown = 2f;
        [Min(0f)] public float fanCooldown = 3f;
        [Header("Ground AoE")]
        [Min(0.01f)] public float aoeWindup = 1f;
        [Min(0.1f)] public float aoeRadius = 3f;
        [Min(0f)] public float aoeDamage = 25f;
        [Min(0f)] public float aoeCooldown = 4f;
        [Header("Phase 2")]
        [Range(0.01f, 0.99f)] public float phaseTwoHealthRatio = 0.5f;
        [Min(0.1f)] public float phaseTwoStraightSpeed = 12f;
        [Min(0.1f)] public float phaseTwoAoERadius = 3.5f;
    }
}
