using System.Text;
using UnityEngine;
using AshenTrial;

public static class EnableBossAndBait
{
    public static string Main()
    {
        var log = new StringBuilder();
        var flow = Object.FindAnyObjectByType<GameFlowController>();
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        var player = GameObject.Find("Player");
        var boss = GameObject.Find("Boss01");
        if (boss == null)
        {
            var all = Object.FindObjectsByType<BossController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == "Boss01")
                {
                    boss = all[i].gameObject;
                    break;
                }
            }
        }

        if (pillar == null || player == null || boss == null)
            return "missing pillar/player/boss flow=" + (flow != null ? flow.State.ToString() : "null");

        boss.SetActive(true);
        var playerHealth = player.GetComponent<Health>();
        playerHealth.DamageBlocked = () => true;

        Vector3 pillarPos = pillar.transform.position;
        Vector3 dir = new Vector3(1f, 0f, -1f).normalized;
        Teleport(player, pillarPos + dir * 2.15f);
        Teleport(boss, pillarPos - dir * 6.15f);
        boss.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);

        var bossAi = boss.GetComponent<BossController>();
        log.AppendLine("flow=" + flow.State);
        log.AppendLine("playerHp=" + playerHealth.CurrentHp + " dead=" + playerHealth.IsDead);
        log.AppendLine("broken=" + pillar.IsBroken);
        log.AppendLine("bossState=" + bossAi.State + " pattern=" + bossAi.CurrentPattern + " phase=" + bossAi.Phase);
        log.AppendLine("distBossPlayer=" + Vector3.Distance(boss.transform.position, player.transform.position).ToString("0.00"));
        log.AppendLine("distBossPillar=" + Vector3.Distance(boss.transform.position, pillarPos).ToString("0.00"));
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
