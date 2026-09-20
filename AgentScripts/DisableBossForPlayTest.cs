using UnityEngine;

public static class DisableBossForPlayTest
{
    public static string Main()
    {
        var boss = GameObject.Find("Boss01");
        if (boss == null) return "missing Boss01";
        boss.SetActive(false);
        return "Boss01 active=" + boss.activeSelf + " play=" + Application.isPlaying;
    }
}
