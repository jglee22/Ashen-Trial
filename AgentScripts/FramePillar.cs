using UnityEditor;
using UnityEngine;

public static class FramePillar
{
    public static string Main()
    {
        GameObject root = GameObject.Find("DestructiblePillar");
        if (root == null) return "missing";
        Selection.activeGameObject = root;
        SceneView view = SceneView.lastActiveSceneView;
        if (view == null && SceneView.sceneViews.Count > 0)
            view = (SceneView)SceneView.sceneViews[0];
        if (view != null)
        {
            view.LookAt(root.transform.position + Vector3.up * 2f, Quaternion.Euler(20f, 140f, 0f), 8f);
            view.Repaint();
        }
        return "framed";
    }
}
