public partial class InteractController
{
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

        if (action == InteractionInspectInputAction.Confirm)
        {
            bool confirmed = inspectedTarget.TryConfirmInspectAction();

            if (!confirmed)
            {
                return;
            }

            CloseInspection();
            _currentTarget = InteractionTarget.Empty;
            ApplyHoverInfo(InteractionHoverInfo.Empty);
            return;
        }

        if (action == InteractionInspectInputAction.Deny)
        {
            CloseInspection();
        }
    }

    private void HandleGenericInteractableInput()
    {
        InteractionInputState inputState = ReadInputState();

        if (_inputService == null || !_inputService.TryGetGenericInteractable(_currentTarget, inputState, out IInteractable interactable))
        {
            return;
        }

        interactable.Interact();
    }

    private InteractionInputState ReadInputState()
    {
        return _inputReader != null ? _inputReader.Read() : InteractionInputState.Empty;
    }
}