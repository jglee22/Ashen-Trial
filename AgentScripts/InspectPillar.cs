using System.Text;
using UnityEngine;

public static class InspectPillar
{
    public static string Main()
    {
        var log = new StringBuilder();
        GameObject root = GameObject.Find("DestructiblePillar");
        if (root == null) return "DestructiblePillar not found";
        Dump(root.transform, 0, log);
        return log.ToString();
    }

    static void Dump(Transform t, int depth, StringBuilder log)
    {
        Renderer renderer = t.GetComponent<Renderer>();
        MeshFilter filter = t.GetComponent<MeshFilter>();
        string extra = "";
        if (filter != null && filter.sharedMesh != null)
            extra += " mesh=" + filter.sharedMesh.name + " verts=" + filter.sharedMesh.vertexCount;
        if (renderer != null)
            extra += " bounds=" + renderer.bounds + " enabled=" + renderer.enabled;
        extra += " active=" + t.gameObject.activeSelf + " pos=" + t.position + " localScale=" + t.localScale;
        log.AppendLine(new string(' ', depth * 2) + t.name + extra);
        for (int i = 0; i < t.childCount; i++)
            Dump(t.GetChild(i), depth + 1, log);
    }
}
