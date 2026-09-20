using System.Text;
using UnityEngine;
using AshenTrial;

public static class SnapshotChunks
{
    public static string Main()
    {
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        var boss = GameObject.Find("Boss01");
        var player = GameObject.Find("Player");
        if (pillar == null) return "no pillar";
        var log = new StringBuilder();
        log.AppendLine("t=" + Time.time.ToString("0.00") + " broken=" + pillar.IsBroken);
        log.AppendLine("intact=" + pillar.transform.Find("Intact").gameObject.activeInHierarchy);
        log.AppendLine("fractured=" + pillar.transform.Find("FracturedRoot").gameObject.activeInHierarchy);
        if (boss != null)
        {
            var ai = boss.GetComponent<BossController>();
            log.AppendLine("boss pos=" + boss.transform.position + " state=" + ai.State + " pattern=" + ai.CurrentPattern + " phase=" + ai.Phase);
        }
        else log.AppendLine("boss=null");
        if (player != null) log.AppendLine("player pos=" + player.transform.position);
        var bodies = pillar.GetComponentsInChildren<Rigidbody>(true);
        Vector3 pillarPos = pillar.transform.position;
        for (int i = 0; i < bodies.Length; i++)
        {
            var b = bodies[i];
            Vector3 offset = b.position - pillarPos;
            log.AppendLine(b.name + " offset=" + offset.ToString("F2") + " v=" + b.linearVelocity.ToString("F2") + " y=" + b.position.y.ToString("F2") + " kin=" + b.isKinematic);
        }
        var dust = pillar.GetComponentInChildren<ParticleSystem>(true);
        if (dust != null)
            log.AppendLine("dust playing=" + dust.isPlaying + " pos=" + dust.transform.position);
        return log.ToString();
    }
}
