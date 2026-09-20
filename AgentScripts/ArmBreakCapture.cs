using System.IO;
using UnityEditor;
using UnityEngine;
using AshenTrial;

public static class ArmBreakCapture
{
    const string CapturePath = "AgentScripts/break_sfx_capture.txt";
    static bool armed;
    static bool sawBroken;
    static string preBreak = "none";

    public static string Arm()
    {
        armed = true;
        sawBroken = false;
        preBreak = "none";
        File.WriteAllText(CapturePath, "armed waiting\n");
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        Tick();
        return "armed " + File.ReadAllText(CapturePath).Replace("\n", " | ");
    }

    public static string Read()
    {
        if (!File.Exists(CapturePath)) return "no capture file";
        return File.ReadAllText(CapturePath);
    }

    static void BlockPlayerDamage()
    {
        var player = GameObject.Find("Player");
        if (player == null) return;
        var health = player.GetComponent<Health>();
        if (health != null) health.DamageBlocked = () => true;
    }

    static bool CurrentBroken()
    {
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        return pillar != null && pillar.IsBroken;
    }

    static void Tick()
    {
        if (!armed) return;
        BlockPlayerDamage();
        string snap = Snapshot();
        bool broken = CurrentBroken();
        if (!broken)
        {
            preBreak = snap;
            File.WriteAllText(CapturePath, "waiting\n--- preBreak ---\n" + preBreak + "\n--- atBreak ---\nwaiting");
            return;
        }

        if (!sawBroken)
        {
            File.WriteAllText(CapturePath, "sawBroken=True\n--- preBreak ---\n" + preBreak + "\n--- atBreak ---\n" + snap);
            sawBroken = true;
            armed = false;
            EditorApplication.update -= Tick;
        }
    }

    static string Snapshot()
    {
        var audio = Object.FindAnyObjectByType<GameAudio>();
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        var bossGo = GameObject.Find("Boss01");
        AudioClip clip = null;
        AudioSource sfx = null;
        ParticleSystem dust = null;
        if (audio != null)
        {
            var so = new SerializedObject(audio);
            clip = so.FindProperty("stoneBreak").objectReferenceValue as AudioClip;
            sfx = so.FindProperty("sfxSource").objectReferenceValue as AudioSource;
        }
        if (pillar != null)
        {
            var pillarSo = new SerializedObject(pillar);
            dust = pillarSo.FindProperty("dust").objectReferenceValue as ParticleSystem;
        }

        string bossLine = "boss=null";
        if (bossGo != null)
        {
            var ai = bossGo.GetComponent<BossController>();
            bossLine = "bossState=" + ai.State + " pattern=" + ai.CurrentPattern + " phase=" + ai.Phase +
                " distPillar=" + (pillar != null ? Vector3.Distance(bossGo.transform.position, pillar.transform.position).ToString("0.00") : "na");
        }

        return "t=" + Time.time.ToString("0.000") +
            " scale=" + Time.timeScale.ToString("0.00") +
            " broken=" + (pillar != null && pillar.IsBroken) +
            " stoneBreak=" + (clip != null ? clip.name : "null") +
            " sfxPlaying=" + (sfx != null && sfx.isPlaying) +
            " sfxClipProp=" + (sfx != null && sfx.clip != null ? sfx.clip.name : "none") +
            " sfxLoop=" + (sfx != null && sfx.loop) +
            " sfxPlayOnAwake=" + (sfx != null && sfx.playOnAwake) +
            " sfxPitch=" + (sfx != null ? sfx.pitch.ToString("0.00") : "null") +
            " dustPlaying=" + (dust != null && dust.isPlaying) +
            " dustCount=" + (dust != null ? dust.particleCount.ToString() : "null") +
            " " + bossLine;
    }
}
