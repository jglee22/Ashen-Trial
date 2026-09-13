using UnityEngine;

namespace AshenTrial
{
    internal static class BossLocomotion
    {
        private const float GroundedVerticalSpeed = -2f;

        public static void IgnoreTargetChildSolidColliders(CharacterController body, Transform target)
        {
            foreach (Collider other in target.GetComponentsInChildren<Collider>())
            {
                if (other.transform != target && !other.isTrigger)
                    Physics.IgnoreCollision(body, other);
            }
        }

        public static float TickGravity(CharacterController body, float verticalSpeed, float deltaTime)
        {
            if (body.isGrounded && verticalSpeed < 0f) verticalSpeed = GroundedVerticalSpeed;
            verticalSpeed += Physics.gravity.y * deltaTime;
            return verticalSpeed;
        }

        public static Vector3 ClampToArena(Vector3 position, CharacterController body,
            Vector2 arenaMin, Vector2 arenaMax, float boundaryMargin)
        {
            float margin = body.radius + body.skinWidth + boundaryMargin;
            position.x = Mathf.Clamp(position.x, arenaMin.x + margin, arenaMax.x - margin);
            position.z = Mathf.Clamp(position.z, arenaMin.y + margin, arenaMax.y - margin);
            return position;
        }
    }
}
