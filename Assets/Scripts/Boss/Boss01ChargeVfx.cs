using UnityEngine;

namespace AshenTrial
{
    [DefaultExecutionOrder(150)]
    [DisallowMultipleComponent]
    public sealed class Boss01ChargeVfx : MonoBehaviour
    {
        [SerializeField] private BossController boss;
        [SerializeField] private Health health;
        [SerializeField] private ParticleSystem moveDust;
        [SerializeField] private ParticleSystem impactDust;

        private bool movePlaying;
        private bool impactPlayed;

        private void Awake()
        {
            if (boss == null || health == null || moveDust == null || impactDust == null)
            {
                Debug.LogError("Boss01ChargeVfx: Boss, health and both dust particle systems are required.", this);
                enabled = false;
                return;
            }
            StopMove(true);
        }

        private void OnDisable()
        {
            StopMove(true);
            if (impactDust != null)
                impactDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            impactPlayed = false;
        }

        private void LateUpdate()
        {
            bool charging = IsCharging();
            if (charging)
            {
                if (!movePlaying) StartMove();
            }
            else if (movePlaying)
            {
                StopMove(false);
                impactPlayed = false;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!IsCharging() || impactPlayed || hit == null || hit.collider == null) return;
            if (hit.collider.isTrigger) return;
            if (hit.collider.transform.IsChildOf(transform)) return;
            if (hit.collider.GetComponentInParent<DestructiblePillar>() != null) return;
            if (hit.collider.GetComponentInParent<PlayerCombat>() != null) return;
            Vector3 normal = hit.normal;
            if (Mathf.Abs(normal.y) >= 0.5f) return;
            normal.y = 0f;
            if (normal.sqrMagnitude < 0.0001f) return;
            PlayImpact(hit.point, normal.normalized);
        }

        private bool IsCharging()
        {
            if (!isActiveAndEnabled || boss == null || !boss.isActiveAndEnabled) return false;
            if (health != null && health.IsDead) return false;
            return boss.CurrentPattern == BossController.Pattern.Charge &&
                boss.Phase == BossController.AttackPhase.Active;
        }

        private void StartMove()
        {
            movePlaying = true;
            impactPlayed = false;
            moveDust.Clear(true);
            moveDust.Play(true);
        }

        private void StopMove(bool clear)
        {
            movePlaying = false;
            if (moveDust == null) return;
            if (moveDust.isPlaying || moveDust.particleCount > 0)
                moveDust.Stop(true, clear
                    ? ParticleSystemStopBehavior.StopEmittingAndClear
                    : ParticleSystemStopBehavior.StopEmitting);
        }

        private void PlayImpact(Vector3 point, Vector3 wallNormal)
        {
            impactPlayed = true;
            impactDust.transform.SetPositionAndRotation(
                point,
                Quaternion.LookRotation(wallNormal, Vector3.up));
            impactDust.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            impactDust.Play(true);
        }
    }
}
