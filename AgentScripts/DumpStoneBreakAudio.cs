using UnityEngine;
using AshenTrial;

public static class DumpStoneBreakAudio
{
    public static string Main()
    {
        var audio = Object.FindAnyObjectByType<GameAudio>();
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        if (audio == null) return "no GameAudio";

        var so = new UnityEditor.SerializedObject(audio);
        var clip = so.FindProperty("stoneBreak").objectReferenceValue as AudioClip;
        var sfx = so.FindProperty("sfxSource").objectReferenceValue as AudioSource;
        return "t=" + Time.time.ToString("0.00") +
            " play=" + Application.isPlaying +
            " broken=" + (pillar != null && pillar.IsBroken) +
            " stoneBreak=" + (clip != null ? clip.name : "null") +
            " guidPath=" + (clip != null ? UnityEditor.AssetDatabase.GetAssetPath(clip) : "null") +
            " sfxPlaying=" + (sfx != null && sfx.isPlaying) +
            " sfxClip=" + (sfx != null && sfx.clip != null ? sfx.clip.name : "none") +
            " sfxPlayOnAwake=" + (sfx != null && sfx.playOnAwake) +
            " sfxLoop=" + (sfx != null && sfx.loop);
    }
}
