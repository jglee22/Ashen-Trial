using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AshenTrial;

public static class AssignStoneBreakClip
{
    const string ClipPath = "Assets/Casual Game Sounds U6/CasualGameSounds/DM-CGS-10.wav";
    const string ScenePath = "Assets/Scenes/Main.unity";

    public static string Main()
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ClipPath);
        if (clip == null)
            return "missing clip at " + ClipPath;

        var scene = EditorSceneManager.OpenScene(ScenePath);
        var audio = Object.FindAnyObjectByType<GameAudio>();
        if (audio == null)
            return "GameAudio missing";

        SerializedObject so = new SerializedObject(audio);
        var prop = so.FindProperty("stoneBreak");
        Object previous = prop.objectReferenceValue;
        prop.objectReferenceValue = clip;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(audio);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        var sfx = so.FindProperty("sfxSource").objectReferenceValue as AudioSource;
        return "previous=" + (previous != null ? previous.name : "null") +
            " now=" + clip.name +
            " path=" + ClipPath +
            " length=" + clip.length.ToString("0.000") +
            " sfxPlayOnAwake=" + (sfx != null && sfx.playOnAwake) +
            " sfxLoop=" + (sfx != null && sfx.loop) +
            " sfxPitch=" + (sfx != null ? sfx.pitch.ToString("0.00") : "null") +
            " sfxVolume=" + (sfx != null ? sfx.volume.ToString("0.00") : "null") +
            " saved=" + scene.path;
    }
}
