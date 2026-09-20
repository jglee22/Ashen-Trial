using System.Text;
using UnityEngine;
using AshenTrial;

public static class PostBreakChecks
{
    public static string Main()
    {
        var log = new StringBuilder();
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        var player = GameObject.Find("Player");
        var boss = GameObject.Find("Boss01");
        bool before = pillar.IsBroken;
        bool second = pillar.TryBreak(boss.transform.position, boss.transform.forward);
        log.AppendLine("alreadyBroken=" + before + " secondTryBreak=" + second + " stillBroken=" + pillar.IsBroken);

        var debris = pillar.GetComponentsInChildren<Rigidbody>(true);
        int debrisActive = 0;
        int colliderOn = 0;
        for (int i = 0; i < debris.Length; i++)
        {
            if (debris[i].name.StartsWith("Debris_") && debris[i].gameObject.activeInHierarchy) debrisActive++;
            var col = debris[i].GetComponent<Collider>();
            if (col != null && col.enabled && debris[i].gameObject.activeInHierarchy) colliderOn++;
            log.AppendLine(debris[i].name + " active=" + debris[i].gameObject.activeInHierarchy + " col=" + (col != null && col.enabled) + " kin=" + debris[i].isKinematic);
        }
        log.AppendLine("debrisActive=" + debrisActive + " enabledColliders=" + colliderOn);

        Vector3 start = player.transform.position;
        var body = player.GetComponent<CharacterController>();
        Vector3 move = new Vector3(-2f, 0f, 2f);
        body.Move(move);
        log.AppendLine("playerMove start=" + start + " after=" + player.transform.position + " delta=" + (player.transform.position - start));
        return log.ToString();
    }
}
