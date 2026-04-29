public static class PlayerNeedsLocomotionMultiplierResolver
{
    public static float Resolve(PlayerLocomotionState locomotionState, float idleMultiplier, float walkMultiplier, float runMultiplier)
    {
        return locomotionState switch
        {
            PlayerLocomotionState.Idle => idleMultiplier,
            PlayerLocomotionState.Walking => walkMultiplier,
            PlayerLocomotionState.Running => runMultiplier,
            _ => 1f
        };
    }
}