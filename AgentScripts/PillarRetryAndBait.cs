using System.Text;
using UnityEngine;
using AshenTrial;

public static class PillarRetryAndBait
{
    public static string Main()
    {
        var log = new StringBuilder();
        var flow = Object.FindAnyObjectByType<GameFlowController>();
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        var player = GameObject.Find("Player");
        var boss = GameObject.Find("Boss01");
        if (pillar == null || player == null || boss == null)
            return "missing pillar/player/boss flow=" + (flow != null ? flow.State.ToString() : "null");

        var playerHealth = player.GetComponent<Health>();
        playerHealth.DamageBlocked = () => true;

        log.AppendLine("retryCheck flow=" + flow.State);
        log.AppendLine("playerHp=" + playerHealth.CurrentHp + " dead=" + playerHealth.IsDead);
        log.AppendLine("broken=" + pillar.IsBroken);
        log.AppendLine("intact=" + pillar.transform.Find("Intact").gameObject.activeInHierarchy);
        log.AppendLine("fractured=" + pillar.transform.Find("FracturedRoot").gameObject.activeInHierarchy);

        Vector3 pillarPos = pillar.transform.position;
        Teleport(player, pillarPos + Vector3.right * 0.6f);
        log.AppendLine("playerTouchBroken=" + pillar.IsBroken);

        var bossAi = boss.GetComponent<BossController>();
        Teleport(boss, pillarPos + Vector3.left * 1.0f);
        log.AppendLine("bossTouch state=" + bossAi.State + " pattern=" + bossAi.CurrentPattern + " phase=" + bossAi.Phase + " broken=" + pillar.IsBroken);

        Vector3 dir = new Vector3(1f, 0f, -1f).normalized;
        Teleport(player, pillarPos + dir * 2.15f);
        Teleport(boss, pillarPos - dir * 6.15f);
        boss.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        log.AppendLine("bait player=" + player.transform.position);
        log.AppendLine("bait boss=" + boss.transform.position);
        log.AppendLine("distBossPlayer=" + Vector3.Distance(boss.transform.position, player.transform.position).ToString("0.00"));
        log.AppendLine("distBossPillar=" + Vector3.Distance(boss.transform.position, pillarPos).ToString("0.00"));
        log.AppendLine("bossState=" + bossAi.State + " pattern=" + bossAi.CurrentPattern + " phase=" + bossAi.Phase);
        log.AppendLine("brokenFinal=" + pillar.IsBroken);
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
