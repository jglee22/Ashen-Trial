using System.Text;
using UnityEngine;
using AshenTrial;

public static class PillarNonBreakAndBait
{
    public static string Main()
    {
        var log = new StringBuilder();
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        var boss = GameObject.Find("Boss01");
        var player = GameObject.Find("Player");
        var pillarPos = pillar.transform.position;
        var playerCc = player.GetComponent<CharacterController>();
        var bossCc = boss.GetComponent<CharacterController>();
        var bossAi = boss.GetComponent<BossController>();

        log.AppendLine("before playerPos=" + player.transform.position + " bossPos=" + boss.transform.position);
        log.AppendLine("bossState=" + bossAi.State + " pattern=" + bossAi.CurrentPattern + " phase=" + bossAi.Phase + " broken=" + pillar.IsBroken);

        // Player contact
        Teleport(player, pillarPos + Vector3.right * 0.55f);
        log.AppendLine("afterPlayerTouch pos=" + player.transform.position + " broken=" + pillar.IsBroken);

        // Boss non-charge contact: keep pattern none/chase by being far in phase ready
        // Place boss against pillar while not in Charge Active (it may still be chasing)
        Teleport(boss, pillarPos + Vector3.left * 0.9f);
        log.AppendLine("afterBossTouch pos=" + boss.transform.position + " state=" + bossAi.State + " pattern=" + bossAi.CurrentPattern + " phase=" + bossAi.Phase + " broken=" + pillar.IsBroken);

        // Player attack overlap: activate hitbox near pillar without Charge
        var hitbox = player.GetComponentInChildren<AttackHitbox>(true);
        if (hitbox != null)
        {
            hitbox.BeginSwing(25f, player.transform);
            hitbox.SetActive(true);
            log.AppendLine("playerHitboxOn broken=" + pillar.IsBroken);
            hitbox.SetActive(false);
        }

        // Bait positions for charge: player behind pillar, boss 6.1m in front
        Vector3 dir = new Vector3(1f, 0f, -1f).normalized;
        Vector3 playerPos = pillarPos + dir * 2.15f;
        Vector3 bossPos = pillarPos - dir * 6.15f;
        playerPos.y = 0f;
        bossPos.y = 0f;
        Teleport(player, playerPos);
        Teleport(boss, bossPos);
        boss.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        log.AppendLine("bait player=" + player.transform.position);
        log.AppendLine("bait boss=" + boss.transform.position);
        log.AppendLine("distBossPlayer=" + Vector3.Distance(boss.transform.position, player.transform.position).ToString("0.00"));
        log.AppendLine("distBossPillar=" + Vector3.Distance(boss.transform.position, pillarPos).ToString("0.00"));
        log.AppendLine("state=" + bossAi.State + " pattern=" + bossAi.CurrentPattern + " phase=" + bossAi.Phase);
        log.AppendLine("intact=" + pillar.transform.Find("Intact").gameObject.activeInHierarchy);
        log.AppendLine("fractured=" + pillar.transform.Find("FracturedRoot").gameObject.activeInHierarchy);
        log.AppendLine("broken=" + pillar.IsBroken);
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
