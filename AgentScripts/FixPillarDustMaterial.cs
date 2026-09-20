using UnityEditor;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;

public static class FixPillarDustMaterial
{
    const string DustMaterialPath = "Assets/VFX/Environment/PillarDust.mat";
    const string DustPrefabPath = "Assets/VFX/Environment/PillarDust.prefab";

    public static string Main()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(DustMaterialPath);
        if (material == null) return "missing material";

        if (material.HasProperty("_Surface"))
            material.SetFloat("_Surface", 1f);
        if (material.HasProperty("_Blend"))
            material.SetFloat("_Blend", (float)BaseShaderGUI.BlendMode.Additive);
        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", new Color(0.52f, 0.44f, 0.36f, 1f));
        if (material.HasProperty("_Color"))
            material.SetColor("_Color", new Color(0.52f, 0.44f, 0.36f, 1f));
        BaseShaderGUI.SetupMaterialBlendMode(material);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        EditorUtility.SetDirty(material);

        GameObject contents = PrefabUtility.LoadPrefabContents(DustPrefabPath);
        try
        {
            ParticleSystem particle = contents.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particle.main;
            main.startColor = new Color(0.48f, 0.41f, 0.34f, 0.58f);
            PrefabUtility.SaveAsPrefabAsset(contents, DustPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }

        AssetDatabase.SaveAssets();
        return "blend=additive color=0.52,0.44,0.36 startColor=0.48,0.41,0.34,0.58";
    }
}
