using System.Text;
using UnityEditor;
using UnityEngine;
using AshenTrial;

public static class DumpChargeSfx
{
    public static string Main()
    {
        var log = new StringBuilder();
        var audio = Object.FindAnyObjectByType<GameAudio>();
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        var boss = GameObject.Find("Boss01");
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var health = player.GetComponent<Health>();
            if (health != null) health.DamageBlocked = () => true;
        }

        log.AppendLine("t=" + Time.time.ToString("0.00") + " scale=" + Time.timeScale.ToString("0.00") + " play=" + Application.isPlaying);
        log.AppendLine("broken=" + (pillar != null && pillar.IsBroken));
        if (audio != null)
        {
            var so = new SerializedObject(audio);
            var clip = so.FindProperty("stoneBreak").objectReferenceValue as AudioClip;
            var sfx = so.FindProperty("sfxSource").objectReferenceValue as AudioSource;
            log.AppendLine("stoneBreak=" + (clip != null ? clip.name : "null"));
            log.AppendLine("stoneBreakPath=" + (clip != null ? AssetDatabase.GetAssetPath(clip) : "null"));
            log.AppendLine("stoneBreakLength=" + (clip != null ? clip.length.ToString("0.000") : "null"));
            log.AppendLine("sfxPlaying=" + (sfx != null && sfx.isPlaying));
            log.AppendLine("sfxClipProp=" + (sfx != null && sfx.clip != null ? sfx.clip.name : "none"));
            log.AppendLine("sfxPlayOnAwake=" + (sfx != null && sfx.playOnAwake));
            log.AppendLine("sfxLoop=" + (sfx != null && sfx.loop));
            log.AppendLine("sfxPitch=" + (sfx != null ? sfx.pitch.ToString("0.00") : "null"));
            log.AppendLine("sfxVolume=" + (sfx != null ? sfx.volume.ToString("0.00") : "null"));
        }

        if (pillar != null)
        {
            var pillarSo = new SerializedObject(pillar);
            var dust = pillarSo.FindProperty("dust").objectReferenceValue as ParticleSystem;
            log.AppendLine("dustPlaying=" + (dust != null && dust.isPlaying));
            log.AppendLine("dustCount=" + (dust != null ? dust.particleCount.ToString() : "null"));
            log.AppendLine("intact=" + (pillar.transform.Find("Intact") != null && pillar.transform.Find("Intact").gameObject.activeInHierarchy));
            log.AppendLine("fractured=" + (pillar.transform.Find("FracturedRoot") != null && pillar.transform.Find("FracturedRoot").gameObject.activeInHierarchy));
        }

        if (boss != null)
        {
            var ai = boss.GetComponent<BossController>();
            log.AppendLine("bossState=" + ai.State + " pattern=" + ai.CurrentPattern + " phase=" + ai.Phase);
            if (pillar != null)
                log.AppendLine("distBossPillar=" + Vector3.Distance(boss.transform.position, pillar.transform.position).ToString("0.00"));
        }

        return log.ToString();
    }
}
