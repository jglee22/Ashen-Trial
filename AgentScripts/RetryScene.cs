using UnityEngine;
using AshenTrial;

public static class RetryScene
{
    public static string Main()
    {
        var flow = Object.FindAnyObjectByType<GameFlowController>();
        if (flow == null) return "no flow";
        var state = flow.State;
        if (state == GameFlowController.GameFlowState.GameOver || state == GameFlowController.GameFlowState.RunComplete)
            flow.Retry();
        return "before=" + state;
    }
}
