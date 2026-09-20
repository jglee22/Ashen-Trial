using System.Text;
using UnityEditor;
using UnityEngine;
using AshenTrial;

public static class ValidatePillar
{
    public static string Main()
    {
        var log = new StringBuilder();
        GameObject root = GameObject.Find("DestructiblePillar");
        if (root == null) return "missing pillar";
        DestructiblePillar pillar = root.GetComponent<DestructiblePillar>();
        log.AppendLine("component=" + (pillar != null));
        log.AppendLine("fracturedActive=" + root.transform.Find("FracturedRoot").gameObject.activeSelf);
        log.AppendLine("intactActive=" + root.transform.Find("Intact").gameObject.activeSelf);

        SerializedObject so = new SerializedObject(pillar);
        log.AppendLine("intactRoot=" + NullName(so.FindProperty("intactRoot").objectReferenceValue));
        log.AppendLine("intactCollider=" + NullName(so.FindProperty("intactCollider").objectReferenceValue));
        log.AppendLine("fracturedRoot=" + NullName(so.FindProperty("fracturedRoot").objectReferenceValue));
        log.AppendLine("impactPoint=" + NullName(so.FindProperty("impactPoint").objectReferenceValue));
        log.AppendLine("dust=" + NullName(so.FindProperty("dust").objectReferenceValue));
        log.AppendLine("gameAudio=" + NullName(so.FindProperty("gameAudio").objectReferenceValue));
        log.AppendLine("followCamera=" + NullName(so.FindProperty("followCamera").objectReferenceValue));

        GameAudio audio = Object.FindAnyObjectByType<GameAudio>();
        SerializedObject audioSo = new UnityEditor.SerializedObject(audio);
        log.AppendLine("stoneBreak=" + NullName(audioSo.FindProperty("stoneBreak").objectReferenceValue));

        bool fracturedWasActive = root.transform.Find("FracturedRoot").gameObject.activeSelf;
        root.transform.Find("FracturedRoot").gameObject.SetActive(true);
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].sharedMaterials;
            for (int m = 0; m < mats.Length; m++)
            {
                string shader = mats[m] == null ? "NULL" : mats[m].shader.name;
                log.AppendLine(renderers[i].name + " mat" + m + "=" + (mats[m] == null ? "null" : mats[m].name) + " shader=" + shader);
            }
        }
        Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
        log.AppendLine("rigidbodies=" + bodies.Length);
        int kinematic = 0;
        int convex = 0;
        for (int i = 0; i < bodies.Length; i++)
        {
            if (bodies[i].isKinematic) kinematic++;
            MeshCollider meshCollider = bodies[i].GetComponent<MeshCollider>();
            if (meshCollider != null && meshCollider.convex) convex++;
            log.AppendLine(bodies[i].name + " pos=" + bodies[i].position + " kinematic=" + bodies[i].isKinematic);
        }
        log.AppendLine("kinematic=" + kinematic + " convex=" + convex);
        root.transform.Find("FracturedRoot").gameObject.SetActive(fracturedWasActive);
        return log.ToString();
    }

    static string NullName(Object value)
    {
        return value == null ? "null" : value.name;
    }
}
