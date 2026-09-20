using System.Text;
using UnityEngine;
using AshenTrial;

public static class DumpAfterTryBreak
{
    public static string Main()
    {
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        var boss = GameObject.Find("Boss01");
        var player = GameObject.Find("Player");
        if (pillar == null || boss == null) return "missing";
        if (player != null)
        {
            var health = player.GetComponent<Health>();
            if (health != null) health.DamageBlocked = () => true;
        }

        Vector3 dir = new Vector3(1f, 0f, -1f).normalized;
        bool ok = pillar.TryBreak(boss.transform.position, dir);
        var log = new StringBuilder();
        log.AppendLine("tryBreak=" + ok + " broken=" + pillar.IsBroken + " t=" + Time.time.ToString("0.00"));
        Transform visual = pillar.transform.Find("FracturedRoot/Column_Round_Fractured");
        MeshFilter[] filters = visual.GetComponentsInChildren<MeshFilter>(true);
        log.AppendLine("meshCount=" + filters.Length);
        int kinematicNonBase = 0;
        for (int i = 0; i < filters.Length; i++)
        {
            GameObject chunk = filters[i].gameObject;
            Rigidbody body = chunk.GetComponent<Rigidbody>();
            bool kin = body != null && body.isKinematic;
            if (kin && chunk.name != "Chunk_Base") kinematicNonBase++;
            log.AppendLine(
                chunk.name +
                " active=" + chunk.activeSelf +
                " rb=" + (body != null) +
                " kin=" + kin +
                " grav=" + (body != null && body.useGravity) +
                " v=" + (body != null ? body.linearVelocity.ToString("F2") : "na") +
                " w=" + (body != null ? body.angularVelocity.ToString("F2") : "na") +
                " y=" + chunk.transform.position.y.ToString("F2") +
                " pos=" + chunk.transform.position.ToString("F2"));
        }
        log.AppendLine("nonBaseKinematic=" + kinematicNonBase);
        return log.ToString();
    }
}
