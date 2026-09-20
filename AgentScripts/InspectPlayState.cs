using System.Text;
using UnityEngine;
using AshenTrial;

public static class InspectPlayState
{
    public static string Main()
    {
        var log = new StringBuilder();
        log.AppendLine("time=" + Time.time);
        var flow = Object.FindAnyObjectByType<GameFlowController>();
        log.AppendLine("flow=" + (flow != null ? flow.State.ToString() : "null"));
        foreach (var name in new[] { "Player", "Boss01", "Boss02", "Boss03", "DestructiblePillar" })
        {
            var go = GameObject.Find(name);
            log.AppendLine(name + "=" + (go == null ? "null" : "active pos=" + go.transform.position));
        }
        var player = GameObject.Find("Player");
        if (player != null)
        {
            var health = player.GetComponent<Health>();
            log.AppendLine("playerHp=" + (health != null ? health.CurrentHp + "/" + health.MaxHp + " dead=" + health.IsDead : "no health"));
        }
        var bosses = Object.FindObjectsByType<BossController>(FindObjectsInactive.Include);
        log.AppendLine("bossControllers=" + bosses.Length);
        for (int i = 0; i < bosses.Length; i++)
            log.AppendLine(" boss " + bosses[i].name + " active=" + bosses[i].gameObject.activeInHierarchy + " state=" + bosses[i].State);
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>(FindObjectsInactive.Include);
        log.AppendLine("pillar=" + (pillar == null ? "null" : pillar.name + " broken=" + pillar.IsBroken + " active=" + pillar.gameObject.activeInHierarchy));
        return log.ToString();
    }
}
