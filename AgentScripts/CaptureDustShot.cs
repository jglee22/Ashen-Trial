using UnityEditor;
using UnityEngine;
using AshenTrial;

public static class CaptureDustShot
{
    public static string Main()
    {
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        var cam = Camera.main;
        var boss = GameObject.Find("Boss01");
        var player = GameObject.Find("Player");
        if (pillar == null || cam == null) return "missing pillar/camera";

        if (player != null)
        {
            var health = player.GetComponent<Health>();
            if (health != null) health.DamageBlocked = () => true;
        }

        var follow = cam.GetComponent<PlayerFollowCamera>();
        if (follow != null) follow.enabled = false;
        Vector3 look = pillar.transform.position + new Vector3(0.4f, 1.15f, -0.4f);
        cam.transform.position = pillar.transform.position + new Vector3(4.8f, 2.6f, 3.4f);
        cam.transform.LookAt(look);

        Transform dustTransform = pillar.transform.Find("ImpactPoint/PillarDust");
        ParticleSystem dust = dustTransform != null ? dustTransform.GetComponent<ParticleSystem>() : null;
        bool playingBefore = dust != null && dust.isPlaying;
        int particleBefore = dust != null ? dust.particleCount : -1;

        Vector3 dir = new Vector3(1f, 0f, -1f).normalized;
        bool ok = pillar.TryBreak(boss != null ? boss.transform.position : pillar.transform.position - dir, dir);
        if (dust != null && ok)
            dust.Simulate(0.18f, true, false, true);

        EditorApplication.isPaused = true;
        return "beforePlaying=" + playingBefore +
            " beforeCount=" + particleBefore +
            " tryBreak=" + ok +
            " afterPlaying=" + (dust != null && dust.isPlaying) +
            " afterCount=" + (dust != null ? dust.particleCount : -1) +
            " paused=" + EditorApplication.isPaused +
            " dustPos=" + (dust != null ? dust.transform.position.ToString("F2") : "null");
    }
}
