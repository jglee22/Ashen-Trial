using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using AshenTrial;

public static class FixPillarOrientation
{
    public static string Main()
    {
        var log = new StringBuilder();
        GameObject root = GameObject.Find("DestructiblePillar");
        if (root == null) return "DestructiblePillar not found";

        GameObject intactAsset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/FBX/Column_Round.fbx");
        Transform intactVisual = root.transform.Find("Intact/Column_Round");
        Transform fracturedVisual = root.transform.Find("FracturedRoot/Column_Round_Fractured");
        if (intactVisual == null || fracturedVisual == null || intactAsset == null)
            return "Missing visual roots.";

        intactVisual.localPosition = Vector3.zero;
        intactVisual.localRotation = intactAsset.transform.localRotation;
        intactVisual.localScale = intactAsset.transform.localScale;
        log.AppendLine("Intact rot=" + intactVisual.localEulerAngles + " scale=" + intactVisual.localScale);

        fracturedVisual.localPosition = Vector3.zero;
        fracturedVisual.localRotation = intactVisual.localRotation;
        fracturedVisual.localScale = Vector3.one;
        log.AppendLine("Fractured rot=" + fracturedVisual.localEulerAngles + " scale=" + fracturedVisual.localScale);

        Transform intactRoot = root.transform.Find("Intact");
        CapsuleCollider capsule = root.GetComponentInChildren<CapsuleCollider>(true);
        Bounds intactBounds = Encapsulate(intactRoot.gameObject);
        if (capsule != null)
        {
            Vector3 localCenter = intactRoot.InverseTransformPoint(intactBounds.center);
            capsule.center = localCenter;
            capsule.direction = 1;
            capsule.height = Mathf.Max(1f, intactBounds.size.y);
            capsule.radius = Mathf.Max(0.25f, Mathf.Max(intactBounds.size.x, intactBounds.size.z) * 0.5f);
            log.AppendLine("Capsule h=" + capsule.height + " r=" + capsule.radius + " c=" + capsule.center);
        }

        Transform fracturedRoot = root.transform.Find("FracturedRoot");
        bool fracturedWasActive = fracturedRoot.gameObject.activeSelf;
        fracturedRoot.gameObject.SetActive(true);
        Bounds fracturedBounds = Encapsulate(fracturedVisual.gameObject);
        fracturedRoot.gameObject.SetActive(fracturedWasActive);
        log.AppendLine("Intact world bounds=" + intactBounds);
        log.AppendLine("Fractured world bounds=" + fracturedBounds);

        PrefabUtility.ApplyPrefabInstance(root, InteractionMode.AutomatedAction);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        return log.ToString();
    }

    static Bounds Encapsulate(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = new Bounds(root.transform.position, Vector3.zero);
        bool started = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (!renderers[i].enabled) continue;
            if (!started)
            {
                bounds = renderers[i].bounds;
                started = true;
            }
            else bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }
}
