using UnityEngine;
using AshenTrial;

public static class CallRetry
{
    public static string Main()
    {
        var flow = Object.FindAnyObjectByType<GameFlowController>();
        if (flow == null) return "no flow";
        string before = flow.State.ToString();
        flow.Retry();
        return "before=" + before + " retry invoked";
    }
}
