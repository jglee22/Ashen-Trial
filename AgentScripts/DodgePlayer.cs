using UnityEngine;
using AshenTrial;

public static class DodgePlayer
{
    public static string Main()
    {
        var player = GameObject.Find("Player");
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        var boss = GameObject.Find("Boss01");
        if (player == null || pillar == null) return "missing";
        Vector3 chargeDir = new Vector3(1f, 0f, -1f).normalized;
        if (boss != null)
        {
            Vector3 f = boss.transform.forward;
            f.y = 0f;
            if (f.sqrMagnitude > 0.0001f) chargeDir = f.normalized;
        }
        Vector3 side = Vector3.Cross(Vector3.up, chargeDir).normalized;
        Vector3 dest = player.transform.position + side * 3.2f;
        dest.y = 0f;
        var body = player.GetComponent<CharacterController>();
        if (body != null) body.enabled = false;
        player.transform.position = dest;
        if (body != null) body.enabled = true;
        var ai = boss != null ? boss.GetComponent<BossController>() : null;
        return "dodged to " + dest + " bossState=" + (ai != null ? ai.State + "/" + ai.CurrentPattern + "/" + ai.Phase : "null") + " broken=" + pillar.IsBroken;
    }
}
