using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ReplacePillarDust
{
    const string SourcePrefabPath = "Assets/msVFX_Free Smoke Effects Pack/Prefabs/msVFX_Stylized Smoke 1.prefab";
    const string SourceMaterialPath = "Assets/msVFX_Free Smoke Effects Pack/Materials/msVFX_Stylized Smoke 1_Material.mat";
    const string SourceTexturePath = "Assets/msVFX_Free Smoke Effects Pack/Textures/msVFX_Stylized Smoke 1_Texture.png";
    const string DustPrefabPath = "Assets/VFX/Environment/PillarDust.prefab";
    const string DustMaterialPath = "Assets/VFX/Environment/PillarDust.mat";
    const string PillarPrefabPath = "Assets/Prefabs/Environment/DestructiblePillar.prefab";
    const string ParticleShaderName = "Universal Render Pipeline/Particles/Unlit";

    public static string Main()
    {
        var log = new StringBuilder();
        if (!AssetDatabase.IsValidFolder("Assets/VFX/Environment"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/VFX"))
                AssetDatabase.CreateFolder("Assets", "VFX");
            AssetDatabase.CreateFolder("Assets/VFX", "Environment");
        }

        GameObject sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePrefabPath);
        Texture sourceTexture = AssetDatabase.LoadAssetAtPath<Texture>(SourceTexturePath);
        if (sourcePrefab == null || sourceTexture == null)
            return "missing source smoke prefab or texture";

        if (AssetDatabase.LoadAssetAtPath<GameObject>(DustPrefabPath) != null)
            AssetDatabase.DeleteAsset(DustPrefabPath);
        if (!AssetDatabase.CopyAsset(SourcePrefabPath, DustPrefabPath))
            return "failed to copy smoke prefab";

        Material dustMaterial = CreateDustMaterial(sourceTexture, log);
        ConfigureDustPrefab(dustMaterial, log);
        ReplaceOnPillarPrefab(log);
        AssetDatabase.SaveAssets();
        return log.ToString();
    }

    static Material CreateDustMaterial(Texture sourceTexture, StringBuilder log)
    {
        Shader shader = Shader.Find(ParticleShaderName);
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        Material material = AssetDatabase.LoadAssetAtPath<Material>(DustMaterialPath);
        if (material == null)
        {
            if (AssetDatabase.CopyAsset(SourceMaterialPath, DustMaterialPath))
                material = AssetDatabase.LoadAssetAtPath<Material>(DustMaterialPath);
        }
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, DustMaterialPath);
        }

        material.shader = shader;
        if (material.HasProperty("_BaseMap"))
            material.SetTexture("_BaseMap", sourceTexture);
        if (material.HasProperty("_MainTex"))
            material.SetTexture("_MainTex", sourceTexture);
        Color tint = Color.white;
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", tint);
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", tint);
        if (material.HasProperty("_TintColor"))
            material.SetColor("_TintColor", tint);
        if (material.HasProperty("_Surface"))
            material.SetFloat("_Surface", 1f);
        EditorUtility.SetDirty(material);
        log.AppendLine("material=" + DustMaterialPath + " shader=" + material.shader.name);
        return material;
    }

    static void ConfigureDustPrefab(Material dustMaterial, StringBuilder log)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(DustPrefabPath);
        try
        {
            contents.name = "PillarDust";
            contents.transform.localPosition = Vector3.zero;
            contents.transform.localRotation = Quaternion.Euler(-50f, 0f, 0f);
            contents.transform.localScale = Vector3.one;

            ParticleSystem particle = contents.GetComponent<ParticleSystem>();
            ParticleSystemRenderer renderer = contents.GetComponent<ParticleSystemRenderer>();
            if (particle == null)
                throw new System.InvalidOperationException("copied prefab has no ParticleSystem");

            ParticleSystem.MainModule main = particle.main;
            main.playOnAwake = false;
            main.loop = false;
            main.duration = 0.25f;
            main.startDelay = 0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.8f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.8f, 1.2f);
            main.startColor = new Color(0.50f, 0.43f, 0.36f, 0.55f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 16;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            ParticleSystem.EmissionModule emission = particle.emission;
            emission.enabled = true;
            emission.rateOverTime = 0f;
            emission.rateOverDistance = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });

            ParticleSystem.ShapeModule shape = particle.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 62f;
            shape.radius = 0.22f;
            shape.radiusThickness = 1f;
            shape.rotation = Vector3.zero;

            ParticleSystem.ColorOverLifetimeModule color = particle.colorOverLifetime;
            color.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.86f, 0.80f, 0.70f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.55f, 0.38f),
                    new GradientAlphaKey(0f, 1f)
                });
            color.color = gradient;

            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
                renderer.sharedMaterial = dustMaterial;
            }

            PrefabUtility.SaveAsPrefabAsset(contents, DustPrefabPath);
            log.AppendLine("configured " + DustPrefabPath);
            log.AppendLine("lifetime=" + main.startLifetime.constantMin + "-" + main.startLifetime.constantMax);
            log.AppendLine("size=" + main.startSize.constantMin + "-" + main.startSize.constantMax);
            log.AppendLine("speed=" + main.startSpeed.constantMin + "-" + main.startSpeed.constantMax);
            log.AppendLine("burst=" + emission.GetBurst(0).count.constant);
            log.AppendLine("space=" + main.simulationSpace);
            log.AppendLine("playOnAwake=" + main.playOnAwake + " loop=" + main.loop);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    static void ReplaceOnPillarPrefab(StringBuilder log)
    {
        GameObject dustPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(DustPrefabPath);
        GameObject pillar = PrefabUtility.LoadPrefabContents(PillarPrefabPath);
        try
        {
            Transform impact = pillar.transform.Find("ImpactPoint");
            if (impact == null)
                throw new System.InvalidOperationException("ImpactPoint missing");

            Transform oldDust = impact.Find("StoneDust");
            if (oldDust != null)
            {
                Object.DestroyImmediate(oldDust.gameObject);
                log.AppendLine("removed StoneDust");
            }

            Transform existing = impact.Find("PillarDust");
            if (existing != null)
                Object.DestroyImmediate(existing.gameObject);

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(dustPrefab, impact);
            instance.name = "PillarDust";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.Euler(-50f, 0f, 0f);
            instance.transform.localScale = Vector3.one;

            ParticleSystem particle = instance.GetComponent<ParticleSystem>();
            SerializedObject so = new SerializedObject(pillar.GetComponent<AshenTrial.DestructiblePillar>());
            so.FindProperty("dust").objectReferenceValue = particle;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(pillar, PillarPrefabPath);
            log.AppendLine("wired dust on " + PillarPrefabPath);
            log.AppendLine("impactLocal=" + impact.localPosition);
            log.AppendLine("pillarDustLocal=" + instance.transform.localPosition);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(pillar);
        }
    }
}
