using System.IO;
using UnityEditor;
using UnityEngine;

public static class CaptureOriginalsPreviews
{
    public static string Main()
    {
        string[] names =
        {
            "Column_Round",
            "Column_Square",
            "Column_Round_Short",
            "Arch_Round_RoundColumn",
            "Wall_Broken",
            "Wall_ArchRound_Broken",
            "Bricks",
            "Brick",
            "Statue_Fox",
            "Stairs_2"
        };

        string dir = Path.Combine(Path.GetTempPath(), "AshenTrialOriginalsPreviews");
        Directory.CreateDirectory(dir);
        var lines = new System.Text.StringBuilder();

        foreach (string name in names)
        {
            string path = "Assets/FBX/Originals/" + name + ".fbx";
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                lines.AppendLine(name + " missing");
                continue;
            }

            Texture2D preview = null;
            for (int i = 0; i < 30; i++)
            {
                preview = AssetPreview.GetAssetPreview(asset);
                if (preview != null && !AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID()))
                    break;
                System.Threading.Thread.Sleep(50);
            }
            if (preview == null)
            {
                lines.AppendLine(name + " no preview");
                continue;
            }

            Texture2D copy = CopyReadable(preview);
            string outPath = Path.Combine(dir, name + ".png");
            File.WriteAllBytes(outPath, copy.EncodeToPNG());
            Object.DestroyImmediate(copy);
            lines.AppendLine(name + " " + outPath);
        }
        return lines.ToString();
    }

    static Texture2D CopyReadable(Texture src)
    {
        RenderTexture rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(src, rt);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, src.width, src.height), 0, 0);
        tex.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return tex;
    }
}
