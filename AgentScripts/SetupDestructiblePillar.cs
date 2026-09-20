using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using AshenTrial;

public static class SetupDestructiblePillar
{
    const string ScenePath = "Assets/Scenes/Main.unity";
    const string IntactModelPath = "Assets/FBX/Column_Round.fbx";
    const string FracturedModelPath = "Assets/FBX/Column_Round_Fractured.fbx";
    const string DustMaterialPath = "Assets/VFX/Environment/StoneDust.mat";
    const string PrefabPath = "Assets/Prefabs/Environment/DestructiblePillar.prefab";
    const string StoneBreakClipPath = "Assets/Audio/Omgaudio/giant_step.wav";
    const string ParticleShaderName = "Universal Render Pipeline/Particles/Unlit";

    public static string Main()
    {
        var log = new StringBuilder();
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath);
            log.AppendLine("Opened " + ScenePath);
        }

        GameObject existing = GameObject.Find("DestructiblePillar");
        if (existing != null)
            Object.DestroyImmediate(existing);

        GameObject intactModel = AssetDatabase.LoadAssetAtPath<GameObject>(IntactModelPath);
        GameObject fracturedModel = AssetDatabase.LoadAssetAtPath<GameObject>(FracturedModelPath);
        AudioClip stoneBreak = AssetDatabase.LoadAssetAtPath<AudioClip>(StoneBreakClipPath);
        if (intactModel == null || fracturedModel == null)
            return "Missing column FBX assets.";

        EnsureFolder("Assets/VFX/Environment");
        EnsureFolder("Assets/Prefabs/Environment");
        Material dustMaterial = CreateDustMaterial();

        GameObject root = new GameObject("DestructiblePillar");
        root.transform.SetPositionAndRotation(new Vector3(4f, 0f, -4f), Quaternion.identity);

        GameObject intactRoot = new GameObject("Intact");
        intactRoot.transform.SetParent(root.transform, false);
        GameObject intactVisual = (GameObject)PrefabUtility.InstantiatePrefab(intactModel, intactRoot.transform);
        intactVisual.name = "Column_Round";
        intactVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        intactVisual.transform.localScale = Vector3.one;
        StripImportedColliders(intactVisual);

        GameObject intactColliderObject = new GameObject("IntactCollider");
        intactColliderObject.transform.SetParent(intactRoot.transform, false);
        CapsuleCollider intactCollider = intactColliderObject.AddComponent<CapsuleCollider>();
        Bounds intactBounds = EncapsulateRenderers(intactRoot);
        Vector3 intactLocalCenter = intactRoot.transform.InverseTransformPoint(intactBounds.center);
        intactCollider.center = intactLocalCenter;
        intactCollider.height = Mathf.Max(1f, intactBounds.size.y);
        intactCollider.radius = Mathf.Max(0.2f, Mathf.Max(intactBounds.size.x, intactBounds.size.z) * 0.5f);
        intactCollider.direction = 1;

        GameObject fracturedRoot = new GameObject("FracturedRoot");
        fracturedRoot.transform.SetParent(root.transform, false);
        GameObject fracturedVisual = (GameObject)PrefabUtility.InstantiatePrefab(fracturedModel, fracturedRoot.transform);
        fracturedVisual.name = "Column_Round_Fractured";
        fracturedVisual.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        fracturedVisual.transform.localScale = Vector3.one;
        ConfigureChunks(fracturedVisual);

        GameObject impactPoint = new GameObject("ImpactPoint");
        impactPoint.transform.SetParent(root.transform, false);
        impactPoint.transform.localPosition = new Vector3(0f, 1.15f, 0f);

        GameObject dustObject = new GameObject("StoneDust");
        dustObject.transform.SetParent(impactPoint.transform, false);
        ParticleSystem dust = ConfigureDust(dustObject, dustMaterial);

        DestructiblePillar pillar = root.AddComponent<DestructiblePillar>();
        GameAudio gameAudio = Object.FindFirstObjectByType<GameAudio>();
        PlayerFollowCamera followCamera = Object.FindFirstObjectByType<PlayerFollowCamera>();
        SerializedObject pillarSo = new SerializedObject(pillar);
        pillarSo.FindProperty("intactRoot").objectReferenceValue = intactRoot;
        pillarSo.FindProperty("intactCollider").objectReferenceValue = intactCollider;
        pillarSo.FindProperty("fracturedRoot").objectReferenceValue = fracturedRoot;
        pillarSo.FindProperty("impactPoint").objectReferenceValue = impactPoint.transform;
        pillarSo.FindProperty("dust").objectReferenceValue = dust;
        pillarSo.FindProperty("gameAudio").objectReferenceValue = gameAudio;
        pillarSo.FindProperty("followCamera").objectReferenceValue = followCamera;
        pillarSo.ApplyModifiedPropertiesWithoutUndo();

        if (gameAudio != null && stoneBreak != null)
        {
            SerializedObject audioSo = new SerializedObject(gameAudio);
            audioSo.FindProperty("stoneBreak").objectReferenceValue = stoneBreak;
            audioSo.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(gameAudio);
        }

        fracturedRoot.SetActive(false);
        intactRoot.SetActive(true);
        PrefabUtility.SaveAsPrefabAssetAndConnect(root, PrefabPath, InteractionMode.AutomatedAction);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        int chunkCount = fracturedVisual.GetComponentsInChildren<Rigidbody>(true).Length;
        log.AppendLine("Pillar position: " + root.transform.position);
        log.AppendLine("Intact bounds: " + intactBounds);
        log.AppendLine("Chunk rigidbodies: " + chunkCount);
        log.AppendLine("Dust material: " + (dustMaterial != null ? dustMaterial.shader.name : "null"));
        log.AppendLine("Stone break clip: " + (stoneBreak != null ? stoneBreak.name : "null"));
        log.AppendLine("GameAudio: " + (gameAudio != null ? gameAudio.name : "null"));
        log.AppendLine("Camera: " + (followCamera != null ? followCamera.name : "null"));
        log.AppendLine("Prefab: " + PrefabPath);
        log.AppendLine("Scene saved: " + scene.path);
        return log.ToString();
    }

    static void ConfigureChunks(GameObject fracturedVisual)
    {
        var meshFilters = fracturedVisual.GetComponentsInChildren<MeshFilter>(true);
        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter filter = meshFilters[i];
            if (filter.sharedMesh == null) continue;
            GameObject chunk = filter.gameObject;
            foreach (Collider old in chunk.GetComponents<Collider>())
                Object.DestroyImmediate(old);

            MeshCollider meshCollider = chunk.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = filter.sharedMesh;
            meshCollider.convex = true;

            Rigidbody body = chunk.GetComponent<Rigidbody>();
            if (body == null) body = chunk.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            body.linearDamping = 0.35f;
            body.angularDamping = 0.85f;
            bool isDebris = chunk.name.StartsWith("Debris_");
            bool isBase = chunk.name == "Chunk_Base";
            bool isCapital = chunk.name == "Chunk_Capital";
            body.mass = isDebris ? 1.8f : isBase ? 40f : isCapital ? 14f : 10f;
        }
    }

    static ParticleSystem ConfigureDust(GameObject dustObject, Material material)
    {
        ParticleSystem dust = dustObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = dust.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.8f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.6f, 1.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.55f);
        main.startColor = new Color(0.47f, 0.41f, 0.33f, 0.72f);
        main.gravityModifier = 0.28f;
        main.maxParticles = 48;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.None;

        ParticleSystem.EmissionModule emission = dust.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 28, 36, 1, 0.01f) });

        ParticleSystem.ShapeModule shape = dust.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.42f;
        dustObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        ParticleSystem.ColorOverLifetimeModule color = dust.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(new Color(0.5f, 0.44f, 0.35f), 0f), new GradientColorKey(new Color(0.38f, 0.33f, 0.26f), 1f) },
            new[] { new GradientAlphaKey(0.7f, 0f), new GradientAlphaKey(0.35f, 0.45f), new GradientAlphaKey(0f, 1f) });
        color.color = gradient;

        ParticleSystem.SizeOverLifetimeModule size = dust.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.7f, 1f, 1.4f));

        ParticleSystemRenderer renderer = dustObject.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        if (material != null) renderer.sharedMaterial = material;
        return dust;
    }

    static Material CreateDustMaterial()
    {
        Shader shader = Shader.Find(ParticleShaderName);
        if (shader == null) shader = Shader.Find("Sprites/Default");
        Material material = AssetDatabase.LoadAssetAtPath<Material>(DustMaterialPath);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, DustMaterialPath);
        }
        else
        {
            material.shader = shader;
        }
        material.SetColor("_BaseColor", new Color(0.45f, 0.39f, 0.31f, 0.7f));
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", new Color(0.45f, 0.39f, 0.31f, 0.7f));
        material.SetFloat("_Surface", 1f);
        EditorUtility.SetDirty(material);
        return material;
    }

    static void StripImportedColliders(GameObject root)
    {
        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
            Object.DestroyImmediate(collider);
    }

    static Bounds EncapsulateRenderers(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = new Bounds(root.transform.position, Vector3.zero);
        bool started = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!started)
            {
                bounds = renderers[i].bounds;
                started = true;
            }
            else bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
