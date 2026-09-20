using System.Text;
using UnityEditor;
using UnityEngine;
using AshenTrial;

public static class InspectPillarDust
{
    public static string Main()
    {
        var log = new StringBuilder();
        Dump("PREFAB", PrefabUtility.LoadPrefabContents("Assets/Prefabs/Environment/DestructiblePillar.prefab"), log, true);
        Dump("SCENE", GameObject.Find("DestructiblePillar"), log, false);
        DumpAsset(log);
        return log.ToString();
    }

    static void DumpAsset(StringBuilder log)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/Environment/PillarDust.prefab");
        log.AppendLine("=== ASSET PillarDust ===");
        if (asset == null)
        {
            log.AppendLine("missing");
            return;
        }
        AppendParticle(log, asset.GetComponent<ParticleSystem>(), asset.transform);
    }

    static void Dump(string label, GameObject root, StringBuilder log, bool unload)
    {
        log.AppendLine("=== " + label + " ===");
        try
        {
            if (root == null)
            {
                log.AppendLine("missing");
                return;
            }
            Transform impact = root.transform.Find("ImpactPoint");
            Transform stone = impact != null ? impact.Find("StoneDust") : null;
            Transform dust = impact != null ? impact.Find("PillarDust") : null;
            log.AppendLine("impact=" + (impact != null ? impact.localPosition.ToString() : "null"));
            log.AppendLine("StoneDust=" + (stone != null ? ("active=" + stone.gameObject.activeSelf) : "removed"));
            log.AppendLine("PillarDust=" + (dust != null ? dust.name : "missing"));
            var pillar = root.GetComponent<DestructiblePillar>();
            SerializedObject so = new SerializedObject(pillar);
            var assigned = so.FindProperty("dust").objectReferenceValue as ParticleSystem;
            log.AppendLine("assignedDust=" + (assigned != null ? assigned.name : "null"));
            if (dust != null)
                AppendParticle(log, dust.GetComponent<ParticleSystem>(), dust);
        }
        finally
        {
            if (unload && root != null)
                PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void AppendParticle(StringBuilder log, ParticleSystem particle, Transform transform)
    {
        if (particle == null)
        {
            log.AppendLine("no ParticleSystem");
            return;
        }
        var main = particle.main;
        var emission = particle.emission;
        var shape = particle.shape;
        var renderer = particle.GetComponent<ParticleSystemRenderer>();
        ParticleSystem.Burst burst = emission.burstCount > 0 ? emission.GetBurst(0) : default;
        log.AppendLine("localPos=" + transform.localPosition + " localEuler=" + transform.localEulerAngles);
        log.AppendLine("playOnAwake=" + main.playOnAwake + " loop=" + main.loop + " space=" + main.simulationSpace);
        log.AppendLine("lifetime=" + main.startLifetime.constantMin + "-" + main.startLifetime.constantMax);
        log.AppendLine("size=" + main.startSize.constantMin + "-" + main.startSize.constantMax);
        log.AppendLine("speed=" + main.startSpeed.constantMin + "-" + main.startSpeed.constantMax);
        log.AppendLine("color=" + main.startColor.color);
        log.AppendLine("burst=" + burst.count.constant + " rateOverTime=" + emission.rateOverTime.constant);
        log.AppendLine("shape=" + shape.shapeType + " angle=" + shape.angle + " radius=" + shape.radius);
        log.AppendLine("mat=" + (renderer != null && renderer.sharedMaterial != null ? renderer.sharedMaterial.name : "null"));
    }
}
