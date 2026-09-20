using UnityEngine;
using AshenTrial;

public static class RestoreBossActive
{
    public static string Main()
    {
        var bosses = Object.FindObjectsByType<BossController>(FindObjectsInactive.Include);
        if (bosses == null || bosses.Length == 0) return "no bosses";
        for (int i = 0; i < bosses.Length; i++)
        {
            if (bosses[i].name == "Boss01")
            {
                bool was = bosses[i].gameObject.activeSelf;
                bosses[i].gameObject.SetActive(true);
                return "Boss01 wasActive=" + was + " now=" + bosses[i].gameObject.activeSelf + " play=" + Application.isPlaying;
            }
        }
        return "Boss01 missing among " + bosses.Length;
    }
}
