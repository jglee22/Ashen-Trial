using System.Text;
using UnityEditor;
using UnityEngine;

public static class SearchGravelClips
{
    public static string Main()
    {
        var log = new StringBuilder();
        string[] paths =
        {
            "Assets/Footsteps - Essentials/Footsteps_Gravel",
            "Assets/Footsteps - Essentials/Footsteps_DirtyGround",
            "Assets/Footsteps - Essentials/Footsteps_Tile"
        };
        for (int p = 0; p < paths.Length; p++)
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { paths[p] });
            log.AppendLine("=== " + paths[p] + " count=" + guids.Length + " ===");
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (path.ToLowerInvariant().Contains("walk") && !path.EndsWith("_01.wav") && !path.EndsWith("Land_01.wav"))
                    continue;
                if (path.ToLowerInvariant().Contains("run") && !path.EndsWith("_01.wav"))
                    continue;
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                log.AppendLine(System.IO.Path.GetFileName(path) + " length=" + (clip != null ? clip.length.ToString("0.000") : "?"));
            }
        }
        return log.ToString();
    }
}
