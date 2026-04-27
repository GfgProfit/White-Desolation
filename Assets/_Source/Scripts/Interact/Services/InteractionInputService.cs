public sealed class InteractionInputService
{
    public bool TryGetInspectableTarget(InteractionTarget currentTarget, InteractionInputState inputState, out IInspectableInteractable inspectable)
    {
        inspectable = null;

        if (!inputState.IsInteractPressed)
        {
            return false;
        }

        if (!currentTarget.HasInspectable)
        {
            return false;
        }

        inspectable = currentTarget.Inspectable;
        return inspectable != null;
    }

    public bool TryGetGenericInteractable(InteractionTarget currentTarget, InteractionInputState inputState, out IInteractable interactable)
    {
        interactable = null;

        if (!inputState.IsInteractPressed)
        {
            return false;
        }

        if (currentTarget.HasInspectable)
        {
            return false;
        }

        interactable = currentTarget.Interactable;
        return interactable != null;
    }

    public InteractionInspectInputAction GetInspectInputAction(InteractionInputState inputState)
    {
        if (inputState.IsInteractPressed)
        {
            return InteractionInspectInputAction.Confirm;
        }

        if (inputState.IsInteractDenied)
        {
            return InteractionInspectInputAction.Deny;
        }

        return InteractionInspectInputAction.None;
    }
}