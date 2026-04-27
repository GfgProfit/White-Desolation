public sealed class InteractionInputService
{
    private readonly IPlayerInput _playerInput;

    public InteractionInputService(IPlayerInput playerInput)
    {
        _playerInput = playerInput;
    }

    public bool TryGetInspectableTarget(InteractionTarget currentTarget, out IInspectableInteractable inspectable)
    {
        inspectable = null;

        if (_playerInput == null || !_playerInput.IsInteractPressed())
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

    public bool TryGetGenericInteractable(InteractionTarget currentTarget, out IInteractable interactable)
    {
        interactable = null;

        if (_playerInput == null || !_playerInput.IsInteractPressed())
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

    public InteractionInspectInputAction GetInspectInputAction()
    {
        if (_playerInput == null)
        {
            return InteractionInspectInputAction.None;
        }

        if (_playerInput.IsInteractPressed())
        {
            return InteractionInspectInputAction.Confirm;
        }

        if (_playerInput.IsInteractDenied())
        {
            return InteractionInspectInputAction.Deny;
        }

        return InteractionInspectInputAction.None;
    }
}