using System.Text;
using UnityEngine;
using UnityEditor;

public static class InspectColumnMeshes
{
    public static string Main()
    {
        var log = new StringBuilder();
        DumpAsset("Assets/FBX/Column_Round.fbx", log);
        DumpAsset("Assets/FBX/Column_Round_Fractured.fbx", log);

        GameObject intact = GameObject.Find("DestructiblePillar/Intact/Column_Round");
        if (intact != null)
        {
            MeshFilter filter = intact.GetComponent<MeshFilter>();
            log.AppendLine("Scene intact rotation=" + intact.transform.rotation.eulerAngles +
                " localMeshBounds=" + (filter != null && filter.sharedMesh != null ? filter.sharedMesh.bounds.ToString() : "none"));
        }
        return log.ToString();
    }

    static void DumpAsset(string path, StringBuilder log)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        log.AppendLine("=== " + path + " ===");
        if (importer != null)
        {
            log.AppendLine("globalScale=" + importer.globalScale + " useFileScale=" + importer.useFileScale +
                " bakeAxisConversion=" + importer.bakeAxisConversion);
        }
        if (asset == null)
        {
            log.AppendLine("asset missing");
            return;
        }
        MeshFilter[] filters = asset.GetComponentsInChildren<MeshFilter>(true);
        for (int i = 0; i < filters.Length; i++)
        {
            Mesh mesh = filters[i].sharedMesh;
            log.AppendLine(filters[i].name + " localEuler=" + filters[i].transform.localEulerAngles +
                " lossyScale=" + filters[i].transform.lossyScale +
                " meshBounds=" + (mesh != null ? mesh.bounds.ToString() : "null") +
                " verts=" + (mesh != null ? mesh.vertexCount : 0));
        }
    }
}
