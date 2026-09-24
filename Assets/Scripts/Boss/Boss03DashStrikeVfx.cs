using UnityEngine;

namespace AshenTrial
{
    [DefaultExecutionOrder(150)]
    [DisallowMultipleComponent]
    public sealed class Boss03DashStrikeVfx : MonoBehaviour
    {
        [SerializeField] private Boss03Controller boss;
        [SerializeField] private Health health;
        [SerializeField] private ParticleSystem startBurst;
        [SerializeField] private ParticleSystem slash;
        [SerializeField] private TrailRenderer dashTrail;

        private bool dashing;

        private void Awake()
        {
            if (boss == null || health == null || startBurst == null || slash == null || dashTrail == null)
            {
                Debug.LogError("Boss03DashStrikeVfx: Boss, health, burst, slash and trail references are required.", this);
                enabled = false;
                return;
            }
            StopAll(true);
        }

        private void OnDisable()
        {
            dashing = false;
            StopAll(true);
        }

        private void LateUpdate()
        {
            if (!isActiveAndEnabled || boss == null || !boss.isActiveAndEnabled ||
                (health != null && health.IsDead))
            {
                if (dashing) StopAll(true);
                dashing = false;
                return;
            }

            bool nowDashing = boss.CurrentPattern == Boss03Controller.Pattern.DashStrike &&
                boss.State == Boss03Controller.BossState.Dash;
            if (nowDashing == dashing) return;
            dashing = nowDashing;
            if (dashing) StartDash();
            else StopAll(false);
        }

        private void StartDash()
        {
            PlayBurst(startBurst);
            PlayBurst(slash);
            dashTrail.Clear();
            dashTrail.emitting = true;
        }

        private static void PlayBurst(ParticleSystem system)
        {
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            system.Play(true);
        }

        private void StopAll(bool clear)
        {
            StopOne(startBurst, clear);
            StopOne(slash, clear);
            if (dashTrail == null) return;
            dashTrail.emitting = false;
            if (clear) dashTrail.Clear();
        }

        private static void StopOne(ParticleSystem system, bool clear)
        {
            if (system == null) return;
            if (system.isPlaying || system.particleCount > 0)
                system.Stop(true, clear
                    ? ParticleSystemStopBehavior.StopEmittingAndClear
                    : ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
