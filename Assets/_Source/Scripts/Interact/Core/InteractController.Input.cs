public partial class InteractController
{
    private InteractionInputState ReadInputState()
    {
        return _inputReader != null ? _inputReader.Read() : InteractionInputState.Empty;
    }

    private bool HandleInspectableInput()
    {
        InteractionInputState inputState = ReadInputState();

        if (_inputService == null || !_inputService.TryGetInspectableTarget(_currentTarget, inputState, out IInspectableInteractable inspectable))
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

        InteractionInputState inputState = ReadInputState();
        InteractionInspectInputAction action = _inputService != null ? _inputService.GetInspectInputAction(inputState) : InteractionInspectInputAction.None;
        InteractionInspectActionResult result = _inspectActionService != null ? _inspectActionService.Execute(inspectedTarget, action) : InteractionInspectActionResult.None;

        ApplyInspectActionResult(result);
    }

    private void ApplyInspectActionResult(InteractionInspectActionResult result)
    {
        if (result == InteractionInspectActionResult.None)
        {
            return;
        }

        CloseInspection();

        if (result == InteractionInspectActionResult.CloseAndClearHover)
        {
            _currentTarget = InteractionTarget.Empty;
            ApplyHoverInfo(InteractionHoverInfo.Empty);
        }
    }

    private void HandleGenericInteractableInput()
    {
        InteractionInputState inputState = ReadInputState();

        if (_inputService == null || !_inputService.TryGetGenericInteractable(_currentTarget, inputState, out IInteractable interactable))
        {
            return;
        }

        _executionService?.Execute(interactable);
    }
}