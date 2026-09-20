using System.Text;
using UnityEngine;
using AshenTrial;

public static class PillarPlaySetup
{
    public static string Main()
    {
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        var boss = GameObject.Find("Boss01");
        var player = GameObject.Find("Player");
        if (pillar == null || boss == null || player == null)
            return "missing objects";

        Vector3 pillarPos = pillar.transform.position;
        Vector3 dir = new Vector3(1f, 0f, -1f).normalized;
        Vector3 playerPos = pillarPos + dir * 2.1f;
        Vector3 bossPos = pillarPos - dir * 6.1f;
        playerPos.y = 0f;
        bossPos.y = 0f;

        Teleport(player, playerPos);
        Teleport(boss, bossPos);
        boss.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        var log = new StringBuilder();
        log.AppendLine("pillar=" + pillarPos);
        log.AppendLine("player=" + player.transform.position);
        log.AppendLine("boss=" + boss.transform.position);
        log.AppendLine("broken=" + pillar.IsBroken);
        log.AppendLine("intact=" + pillar.transform.Find("Intact").gameObject.activeInHierarchy);
        log.AppendLine("fractured=" + pillar.transform.Find("FracturedRoot").gameObject.activeInHierarchy);
        var controller = boss.GetComponent<BossController>();
        log.AppendLine("bossState=" + controller.State + " pattern=" + controller.CurrentPattern + " phase=" + controller.Phase);
        log.AppendLine("distanceBossPlayer=" + Vector3.Distance(boss.transform.position, player.transform.position));
        log.AppendLine("distanceBossPillar=" + Vector3.Distance(boss.transform.position, pillarPos));
        return log.ToString();
    }

    static void Teleport(GameObject go, Vector3 pos)
    {
        var body = go.GetComponent<CharacterController>();
        if (body != null) body.enabled = false;
        go.transform.position = pos;
        if (body != null) body.enabled = true;
    }
}
