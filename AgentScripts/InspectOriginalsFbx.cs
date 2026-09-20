using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class InspectOriginalsFbx
{
    const string Folder = "Assets/FBX/Originals";
    const string Baseline = "Column_Round";

    public static string Main()
    {
        var log = new StringBuilder();
        string[] guids = AssetDatabase.FindAssets("t:Model", new[] { Folder });
        var rows = new List<string>();
        GameObject baseline = null;
        Bounds baselineBounds = default;
        int baselineVerts = 0;
        string baselineMats = "";

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) continue;
            MeshFilter[] filters = asset.GetComponentsInChildren<MeshFilter>(true);
            SkinnedMeshRenderer[] skinned = asset.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var mats = new HashSet<string>();
            int verts = 0;
            int tris = 0;
            int meshCount = 0;
            Bounds bounds = new Bounds();
            bool hasBounds = false;
            foreach (var filter in filters)
            {
                if (filter.sharedMesh == null) continue;
                meshCount++;
                verts += filter.sharedMesh.vertexCount;
                tris += filter.sharedMesh.triangles.Length / 3;
                Bounds b = filter.sharedMesh.bounds;
                Vector3 worldMin = filter.transform.TransformPoint(b.min);
                Vector3 worldMax = filter.transform.TransformPoint(b.max);
                Encapsulate(ref bounds, ref hasBounds, worldMin, worldMax);
            }
            foreach (var skin in skinned)
            {
                if (skin.sharedMesh == null) continue;
                meshCount++;
                verts += skin.sharedMesh.vertexCount;
                tris += skin.sharedMesh.triangles.Length / 3;
            }
            foreach (var renderer in asset.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.sharedMaterials == null) continue;
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat != null) mats.Add(mat.name);
                }
            }

            Vector3 size = hasBounds ? bounds.size : Vector3.zero;
            string line = Path.GetFileNameWithoutExtension(path) +
                "\tpath=" + path +
                "\tmeshes=" + meshCount +
                "\tverts=" + verts +
                "\ttris=" + tris +
                "\tsize=" + size.x.ToString("0.00") + "x" + size.y.ToString("0.00") + "x" + size.z.ToString("0.00") +
                "\theight=" + size.y.ToString("0.00") +
                "\tmats=" + string.Join(",", mats.OrderBy(m => m));
            rows.Add(line);

            if (Path.GetFileNameWithoutExtension(path) == Baseline)
            {
                baseline = asset;
                baselineBounds = bounds;
                baselineVerts = verts;
                baselineMats = string.Join(",", mats.OrderBy(m => m));
            }
        }

        log.AppendLine("count=" + rows.Count);
        if (baseline != null)
        {
            log.AppendLine("BASELINE Column_Round height=" + baselineBounds.size.y.ToString("0.00") +
                " size=" + baselineBounds.size.ToString("F2") +
                " verts=" + baselineVerts +
                " mats=" + baselineMats);
        }
        else
        {
            log.AppendLine("BASELINE Column_Round missing in Originals");
        }

        foreach (string row in rows.OrderBy(r => r))
            log.AppendLine(row);
        return log.ToString();
    }

    static void Encapsulate(ref Bounds bounds, ref bool hasBounds, Vector3 a, Vector3 b)
    {
        if (!hasBounds)
        {
            bounds = new Bounds(a, Vector3.zero);
            bounds.Encapsulate(b);
            hasBounds = true;
        }
        else
        {
            bounds.Encapsulate(a);
            bounds.Encapsulate(b);
        }
    }
}
