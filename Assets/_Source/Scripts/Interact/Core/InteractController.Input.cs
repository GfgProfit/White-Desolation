public partial class InteractController
{
    private InteractionInputState ReadInputState()
    {
        return _inputReader != null ? _inputReader.Read() : InteractionInputState.Empty;
    }

    private bool HandleInspectableInput()
    {
        if (_inputService == null || !_inputService.TryGetInspectableTarget(_currentTarget, _currentInputState, out IInspectableInteractable inspectable))
        {
            return false;
        }

        OpenInspection(inspectable);
        return true;
    }

    private void HandleInspectInput()
    {
        IInspectableInteractable inspectedTarget = _inspectSession?.Target;

        if (inspectedTarget == null)
        {
            CloseInspection();
            return;
        }

        InteractionInspectInputAction action = _inputService != null ? _inputService.GetInspectInputAction(_currentInputState) : InteractionInspectInputAction.None;
        InteractionInspectActionResult result = _inspectActionService != null ? _inspectActionService.Resolve(inspectedTarget, action) : InteractionInspectActionResult.None;

        ApplyInspectActionResult(result);
    }

    private void HandleGenericInteractableInput()
    {
        if (_inputService == null || !_inputService.TryGetGenericInteractable(_currentTarget, _currentInputState, out IInteractable interactable))
        {
            return;
        }

        _executionService?.Execute(interactable);
    }
}