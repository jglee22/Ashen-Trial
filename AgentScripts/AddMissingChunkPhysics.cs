using System.Text;
using UnityEditor;
using UnityEngine;

public static class AddMissingChunkPhysics
{
    const string PrefabPath = "Assets/Prefabs/Environment/DestructiblePillar.prefab";

    public static string Main()
    {
        var log = new StringBuilder();
        GameObject prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            Transform visual = prefab.transform.Find("FracturedRoot/Column_Round_Fractured");
            if (visual == null)
                return "FracturedRoot/Column_Round_Fractured missing";

            MeshFilter[] filters = visual.GetComponentsInChildren<MeshFilter>(true);
            int addedRb = 0;
            int addedCol = 0;
            int skipped = 0;
            for (int i = 0; i < filters.Length; i++)
            {
                MeshFilter filter = filters[i];
                if (filter.sharedMesh == null) continue;
                GameObject chunk = filter.gameObject;

                MeshCollider meshCollider = chunk.GetComponent<MeshCollider>();
                if (meshCollider == null)
                {
                    meshCollider = chunk.AddComponent<MeshCollider>();
                    addedCol++;
                }
                meshCollider.sharedMesh = filter.sharedMesh;
                meshCollider.convex = true;

                Rigidbody body = chunk.GetComponent<Rigidbody>();
                if (body == null)
                {
                    body = chunk.AddComponent<Rigidbody>();
                    addedRb++;
                    body.isKinematic = true;
                    body.useGravity = false;
                    body.interpolation = RigidbodyInterpolation.Interpolate;
                    body.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    body.linearDamping = 0.35f;
                    body.angularDamping = 0.85f;
                    bool isDebris = chunk.name.StartsWith("Debris_");
                    bool isBase = chunk.name == "Chunk_Base";
                    bool isCapital = chunk.name == "Chunk_Capital";
                    body.mass = isDebris ? 1.8f : isBase ? 40f : isCapital ? 14f : 10f;
                    log.AppendLine("added " + chunk.name + " mass=" + body.mass);
                }
                else
                {
                    skipped++;
                    log.AppendLine("kept " + chunk.name + " mass=" + body.mass);
                }
            }

            PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath);
            log.AppendLine("addedRb=" + addedRb + " addedCol=" + addedCol + " kept=" + skipped + " meshFilters=" + filters.Length);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefab);
        }

        return log.ToString();
    }
}
