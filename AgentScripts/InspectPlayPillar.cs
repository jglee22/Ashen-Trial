using System.Text;
using UnityEngine;
using AshenTrial;

public static class InspectPlayPillar
{
    public static string Main()
    {
        var log = new StringBuilder();
        var pillars = Object.FindObjectsByType<DestructiblePillar>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        log.AppendLine("pillarCount=" + pillars.Length);
        foreach (var p in pillars)
        {
            log.AppendLine("--- " + p.name + " pos=" + p.transform.position + " broken=" + p.IsBroken);
            for (int i = 0; i < p.transform.childCount; i++)
            {
                var c = p.transform.GetChild(i);
                log.AppendLine(" child " + c.name + " activeSelf=" + c.gameObject.activeSelf + " activeInHierarchy=" + c.gameObject.activeInHierarchy);
            }
            var intact = p.transform.Find("Intact/Column_Round");
            var fractured = p.transform.Find("FracturedRoot/Column_Round_Fractured");
            if (intact != null)
                log.AppendLine(" intact euler=" + intact.localEulerAngles + " scale=" + intact.localScale);
            if (fractured != null)
                log.AppendLine(" fractured euler=" + fractured.localEulerAngles + " scale=" + fractured.localScale + " childCount=" + fractured.childCount);
        }
        return log.ToString();
    }
}
