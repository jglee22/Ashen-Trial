using UnityEngine;
using AshenTrial;

public static class TryBreakAgain
{
    public static string Main()
    {
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        var boss = GameObject.Find("Boss01");
        if (pillar == null || boss == null) return "missing";
        bool before = pillar.IsBroken;
        bool ok = pillar.TryBreak(boss.transform.position, new Vector3(1f, 0f, -1f));
        return "beforeBroken=" + before + " tryBreak=" + ok + " afterBroken=" + pillar.IsBroken;
    }
}
