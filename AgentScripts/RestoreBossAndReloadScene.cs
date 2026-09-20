using UnityEditor.SceneManagement;
using UnityEngine;
using AshenTrial;

public static class RestoreBossAndReloadScene
{
    const string ScenePath = "Assets/Scenes/Main.unity";

    public static string Main()
    {
        var bosses = Object.FindObjectsByType<BossController>(FindObjectsInactive.Include);
        string boss = "none";
        for (int i = 0; i < bosses.Length; i++)
        {
            if (bosses[i].name == "Boss01")
            {
                boss = "wasActive=" + bosses[i].gameObject.activeSelf;
                bosses[i].gameObject.SetActive(true);
                boss += " now=" + bosses[i].gameObject.activeSelf;
                break;
            }
        }

        var scene = EditorSceneManager.OpenScene(ScenePath);
        var restored = GameObject.Find("Boss01");
        var audio = Object.FindAnyObjectByType<GameAudio>();
        var so = new UnityEditor.SerializedObject(audio);
        var clip = so.FindProperty("stoneBreak").objectReferenceValue as AudioClip;
        return "beforeReload " + boss +
            " afterReload boss=" + (restored != null && restored.activeSelf) +
            " stoneBreak=" + (clip != null ? clip.name : "null") +
            " dirty=" + scene.isDirty;
    }
}
