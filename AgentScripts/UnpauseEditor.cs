using UnityEditor;
using UnityEngine;

public static class UnpauseEditor
{
    public static string Main()
    {
        EditorApplication.isPaused = false;
        return "paused=" + EditorApplication.isPaused;
    }
}
