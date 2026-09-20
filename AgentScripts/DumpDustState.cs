using UnityEngine;
using AshenTrial;

public static class DumpDustState
{
    public static string Main()
    {
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        if (pillar == null) return "no pillar";
        Transform dustTransform = pillar.transform.Find("ImpactPoint/PillarDust");
        ParticleSystem dust = dustTransform != null ? dustTransform.GetComponent<ParticleSystem>() : null;
        Transform stone = pillar.transform.Find("ImpactPoint/StoneDust");
        var boss = GameObject.Find("Boss01");
        var ai = boss != null ? boss.GetComponent<BossController>() : null;
        return "t=" + Time.time.ToString("0.00") +
            " broken=" + pillar.IsBroken +
            " stoneDust=" + (stone != null) +
            " playing=" + (dust != null && dust.isPlaying) +
            " count=" + (dust != null ? dust.particleCount : -1) +
            " pos=" + (dust != null ? dust.transform.position.ToString("F2") : "null") +
            " boss=" + (ai != null ? ai.State + "/" + ai.CurrentPattern + "/" + ai.Phase : "null");
    }
}
