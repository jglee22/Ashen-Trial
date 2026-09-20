using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class SearchStoneSfx
{
    static readonly string[] Folders =
    {
        "Assets/Audio/Omgaudio",
        "Assets/Casual Game Sounds U6",
        "Assets/Footsteps - Essentials"
    };

    static readonly string[] Keywords =
    {
        "stone", "rock", "break", "crack", "crumble", "collapse", "smash",
        "impact", "debris", "rubble", "concrete", "heavy", "thud",
        "explosion", "cannon", "crash", "collision", "hammer", "giant_step"
    };

    public static string Main()
    {
        const string query = "t:AudioClip (stone OR rock OR break OR crack OR crumble OR collapse OR smash OR impact OR debris OR rubble OR concrete OR heavy OR thud)";
        var providers = new[]
        {
            UnityEditor.Search.SearchService.GetProvider("asset"),
            UnityEditor.Search.SearchService.GetProvider("scene")
        };
        var preferred = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(providers, p => p != null));
        var context = preferred.Length > 0
            ? new UnityEditor.Search.SearchContext(preferred, query)
            : new UnityEditor.Search.SearchContext(UnityEditor.Search.SearchService.GetActiveProviders(), query);
        UnityEditor.Search.SearchService.ShowWindow(context);

        var log = new StringBuilder();
        log.AppendLine("searchQuery=" + query);
        var hits = new List<string>();
        for (int f = 0; f < Folders.Length; f++)
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { Folders[f] });
            log.AppendLine("folder=" + Folders[f] + " clips=" + guids.Length);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                string lower = path.ToLowerInvariant();
                bool match = false;
                for (int k = 0; k < Keywords.Length; k++)
                {
                    if (lower.Contains(Keywords[k]))
                    {
                        match = true;
                        break;
                    }
                }
                if (!match) continue;
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                float length = clip != null ? clip.length : -1f;
                int samples = clip != null ? clip.samples : 0;
                int freq = clip != null ? clip.frequency : 0;
                int channels = clip != null ? clip.channels : 0;
                long bytes = 0;
                string full = Path.GetFullPath(path);
                if (File.Exists(full))
                    bytes = new FileInfo(full).Length;
                hits.Add(path + " | name=" + (clip != null ? clip.name : "null") +
                    " | length=" + length.ToString("0.000") + "s" +
                    " | samples=" + samples +
                    " | hz=" + freq +
                    " | ch=" + channels +
                    " | bytes=" + bytes);
            }
        }
        hits.Sort();
        log.AppendLine("matches=" + hits.Count);
        for (int i = 0; i < hits.Count; i++)
            log.AppendLine(hits[i]);
        return log.ToString();
    }
}
