using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using AshenTrial;

public static class CaptureBreakImmediate
{
    const string OutputPath = "Temp/break-immediate.txt";
    static bool hooked;
    static bool dumped;
    static float baitTime;

    public static string Main()
    {
        var flow = Object.FindAnyObjectByType<GameFlowController>();
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        var player = GameObject.Find("Player");
        var boss = GameObject.Find("Boss01");
        if (pillar == null || player == null || boss == null)
            return "missing objects flow=" + (flow != null ? flow.State.ToString() : "null");

        player.GetComponent<Health>().DamageBlocked = () => true;
        Vector3 pillarPos = pillar.transform.position;
        Vector3 dir = new Vector3(1f, 0f, -1f).normalized;
        Teleport(player, pillarPos + dir * 2.15f);
        Teleport(boss, pillarPos - dir * 6.15f);
        boss.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        dumped = false;
        baitTime = Time.time;
        if (!hooked)
        {
            EditorApplication.update += Tick;
            hooked = true;
        }
        File.WriteAllText(OutputPath, "waiting broken=" + pillar.IsBroken + " t=" + baitTime.ToString("0.00") + "\n");
        return "hooked baitTime=" + baitTime.ToString("0.00") + " broken=" + pillar.IsBroken;
    }

    static void Tick()
    {
        if (dumped || !Application.isPlaying) return;
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        if (pillar == null || !pillar.IsBroken) return;
        dumped = true;
        EditorApplication.update -= Tick;
        hooked = false;
        File.WriteAllText(OutputPath, Dump(pillar));
    }

    static string Dump(DestructiblePillar pillar)
    {
        var log = new StringBuilder();
        log.AppendLine("t=" + Time.time.ToString("0.00") + " baitAge=" + (Time.time - baitTime).ToString("0.00") + " broken=" + pillar.IsBroken);
        Transform visual = pillar.transform.Find("FracturedRoot/Column_Round_Fractured");
        MeshFilter[] filters = visual.GetComponentsInChildren<MeshFilter>(true);
        log.AppendLine("meshCount=" + filters.Length);
        for (int i = 0; i < filters.Length; i++)
        {
            GameObject chunk = filters[i].gameObject;
            Rigidbody body = chunk.GetComponent<Rigidbody>();
            log.AppendLine(
                chunk.name +
                " active=" + chunk.activeSelf +
                " rb=" + (body != null) +
                " kin=" + (body != null && body.isKinematic) +
                " grav=" + (body != null && body.useGravity) +
                " v=" + (body != null ? body.linearVelocity.ToString("F2") : "na") +
                " w=" + (body != null ? body.angularVelocity.ToString("F2") : "na") +
                " y=" + chunk.transform.position.y.ToString("F2") +
                " pos=" + chunk.transform.position.ToString("F2"));
        }
        return log.ToString();
    }

    static void Teleport(GameObject go, Vector3 pos)
    {
        pos.y = 0f;
        var body = go.GetComponent<CharacterController>();
        if (body != null) body.enabled = false;
        go.transform.position = pos;
        if (body != null) body.enabled = true;
    }
}
