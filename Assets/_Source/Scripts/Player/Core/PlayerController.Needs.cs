public partial class PlayerController
{
    private void UpdateNeedsState(bool isMoving, bool isRunning, bool isCrouching)
    {
        PlayerLocomotionState locomotionState = PlayerLocomotionStateResolver.Resolve(isMoving, isRunning, isCrouching);
        _needsController.SetLocomotionState(locomotionState);
    }
}