using System.Text;
using UnityEditor;
using UnityEngine;

public static class InspectFracturedChunks
{
    public static string Main()
    {
        var log = new StringBuilder();
        DumpAsset(log);
        DumpPrefab(log);
        DumpScene(log);
        return log.ToString();
    }

    static void DumpAsset(StringBuilder log)
    {
        const string path = "Assets/FBX/Column_Round_Fractured.fbx";
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        log.AppendLine("=== FBX " + path + " ===");
        if (asset == null)
        {
            log.AppendLine("missing");
            return;
        }

        log.AppendLine("root=" + asset.name);
        Transform[] children = GetDirectMeshChildren(asset.transform);
        log.AppendLine("childCount=" + children.Length);
        for (int i = 0; i < children.Length; i++)
            AppendChunk(log, "fbx", children[i].gameObject);
    }

    static void DumpPrefab(StringBuilder log)
    {
        const string path = "Assets/Prefabs/Environment/DestructiblePillar.prefab";
        GameObject prefab = PrefabUtility.LoadPrefabContents(path);
        log.AppendLine("=== PREFAB " + path + " ===");
        try
        {
            Transform visual = prefab.transform.Find("FracturedRoot/Column_Round_Fractured");
            if (visual == null)
            {
                log.AppendLine("FracturedRoot/Column_Round_Fractured missing");
                return;
            }

            Transform[] children = GetDirectMeshChildren(visual);
            log.AppendLine("childCount=" + children.Length);
            for (int i = 0; i < children.Length; i++)
                AppendChunk(log, "prefab", children[i].gameObject);

            Rigidbody[] bodies = visual.GetComponentsInChildren<Rigidbody>(true);
            log.AppendLine("prefabRigidbodyCount=" + bodies.Length);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefab);
        }
    }

    static void DumpScene(StringBuilder log)
    {
        GameObject pillar = GameObject.Find("DestructiblePillar");
        log.AppendLine("=== SCENE ===");
        if (pillar == null)
        {
            log.AppendLine("DestructiblePillar missing");
            return;
        }

        Transform visual = pillar.transform.Find("FracturedRoot/Column_Round_Fractured");
        log.AppendLine("pillarActive=" + pillar.activeInHierarchy);
        log.AppendLine("fracturedRootActive=" + (pillar.transform.Find("FracturedRoot") != null && pillar.transform.Find("FracturedRoot").gameObject.activeSelf));
        if (visual == null)
        {
            log.AppendLine("scene visual missing");
            return;
        }

        Transform[] children = GetDirectMeshChildren(visual);
        log.AppendLine("childCount=" + children.Length);
        for (int i = 0; i < children.Length; i++)
            AppendChunk(log, "scene", children[i].gameObject);

        Rigidbody[] bodies = visual.GetComponentsInChildren<Rigidbody>(true);
        log.AppendLine("sceneRigidbodyCount=" + bodies.Length);
        log.AppendLine("playMode=" + Application.isPlaying);
    }

    static Transform[] GetDirectMeshChildren(Transform root)
    {
        var list = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.GetComponent<MeshFilter>() != null)
                list.Add(child);
            else
                logNested(child, list);
        }
        return list.ToArray();
    }

    static void logNested(Transform node, System.Collections.Generic.List<Transform> list)
    {
        if (node.GetComponent<MeshFilter>() != null)
            list.Add(node);
        for (int i = 0; i < node.childCount; i++)
            logNested(node.GetChild(i), list);
    }

    static void AppendChunk(StringBuilder log, string source, GameObject chunk)
    {
        Rigidbody body = chunk.GetComponent<Rigidbody>();
        MeshCollider meshCollider = chunk.GetComponent<MeshCollider>();
        log.AppendLine(source +
            " name=" + chunk.name +
            " active=" + chunk.activeSelf +
            " rb=" + (body != null) +
            " kinematic=" + (body != null && body.isKinematic) +
            " gravity=" + (body != null && body.useGravity) +
            " collider=" + (meshCollider != null) +
            " convex=" + (meshCollider != null && meshCollider.convex) +
            " pos=" + chunk.transform.position.ToString("F3"));
    }
}
