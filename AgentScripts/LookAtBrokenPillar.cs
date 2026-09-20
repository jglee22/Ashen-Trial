using UnityEngine;
using AshenTrial;

public static class LookAtBrokenPillar
{
    public static string Main()
    {
        var pillar = Object.FindAnyObjectByType<DestructiblePillar>();
        var cam = Camera.main;
        if (pillar == null || cam == null) return "missing camera/pillar";

        var follow = cam.GetComponent<PlayerFollowCamera>();
        if (follow != null) follow.enabled = false;

        Vector3 look = pillar.transform.position + new Vector3(1.5f, 0.8f, -1.5f);
        cam.transform.position = pillar.transform.position + new Vector3(6.5f, 4.2f, 3.2f);
        cam.transform.LookAt(look);
        return "cam=" + cam.transform.position + " look=" + look + " broken=" + pillar.IsBroken;
    }
}
