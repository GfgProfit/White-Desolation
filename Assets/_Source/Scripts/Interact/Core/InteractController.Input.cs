public partial class InteractController
{
    private bool HandleInspectableInput()
    {
        if (_inputService == null || !_inputService.TryGetInspectableTarget(_currentTarget, out IInspectableInteractable inspectable))
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

        InteractionInspectInputAction action = _inputService != null ? _inputService.GetInspectInputAction() : InteractionInspectInputAction.None;

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
        if (_inputService == null || !_inputService.TryGetGenericInteractable(_currentTarget, out IInteractable interactable))
        {
            return;
        }

        interactable.Interact();
    }
}