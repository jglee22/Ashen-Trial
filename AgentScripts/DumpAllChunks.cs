using System.Text;
using UnityEngine;
using AshenTrial;

public static class DumpAllChunks
{
    public static string Main()
    {
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        if (pillar == null) return "no pillar";

        var log = new StringBuilder();
        Transform visual = pillar.transform.Find("FracturedRoot/Column_Round_Fractured");
        log.AppendLine("t=" + Time.time.ToString("0.00") + " broken=" + pillar.IsBroken + " play=" + Application.isPlaying);
        log.AppendLine("intact=" + pillar.transform.Find("Intact").gameObject.activeSelf);
        log.AppendLine("fracturedRoot=" + pillar.transform.Find("FracturedRoot").gameObject.activeSelf);
        if (visual == null) return log + "visual missing";

        MeshFilter[] filters = visual.GetComponentsInChildren<MeshFilter>(true);
        log.AppendLine("meshCount=" + filters.Length);
        for (int i = 0; i < filters.Length; i++)
        {
            GameObject chunk = filters[i].gameObject;
            Rigidbody body = chunk.GetComponent<Rigidbody>();
            MeshCollider meshCollider = chunk.GetComponent<MeshCollider>();
            log.AppendLine(
                chunk.name +
                " active=" + chunk.activeSelf +
                " rb=" + (body != null) +
                " kin=" + (body != null && body.isKinematic) +
                " grav=" + (body != null && body.useGravity) +
                " v=" + (body != null ? body.linearVelocity.ToString("F2") : "na") +
                " w=" + (body != null ? body.angularVelocity.ToString("F2") : "na") +
                " y=" + chunk.transform.position.y.ToString("F2") +
                " pos=" + chunk.transform.position.ToString("F2") +
                " col=" + (meshCollider != null) +
                " convex=" + (meshCollider != null && meshCollider.convex));
        }
        return log.ToString();
    }
}
