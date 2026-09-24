using UnityEngine;

namespace AshenTrial
{
    [DefaultExecutionOrder(150)]
    [DisallowMultipleComponent]
    public sealed class Boss02FanShotVfx : MonoBehaviour
    {
        [SerializeField] private Boss02Controller boss;
        [SerializeField] private Health health;
        [SerializeField] private ParticleSystem castFlash;
        [SerializeField] private ParticleSystem castStreaks;

        private bool wasFanWindup;

        private void Awake()
        {
            if (boss == null || health == null || castFlash == null || castStreaks == null)
            {
                Debug.LogError("Boss02FanShotVfx: Boss, health and both particle systems are required.", this);
                enabled = false;
                return;
            }
            StopAll(true);
        }

        private void OnDisable()
        {
            wasFanWindup = false;
            StopAll(true);
        }

        private void LateUpdate()
        {
            if (!isActiveAndEnabled || boss == null || !boss.isActiveAndEnabled ||
                (health != null && health.IsDead))
            {
                wasFanWindup = false;
                StopAll(true);
                return;
            }

            bool fanWindup = boss.CurrentPattern == Boss02Controller.Pattern.FanShot &&
                boss.State == Boss02Controller.BossState.Windup;
            bool fanFired = wasFanWindup &&
                boss.CurrentPattern == Boss02Controller.Pattern.FanShot &&
                boss.State == Boss02Controller.BossState.Recovery;
            wasFanWindup = fanWindup;
            if (fanFired) PlayCast();
        }

        private void PlayCast()
        {
            PlayBurst(castFlash);
            PlayBurst(castStreaks);
        }

        private static void PlayBurst(ParticleSystem system)
        {
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            system.Play(true);
        }

        private void StopAll(bool clear)
        {
            StopOne(castFlash, clear);
            StopOne(castStreaks, clear);
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
