public readonly struct InteractionInputState
{
    public readonly bool IsInteractPressed;
    public readonly bool IsInteractDenied;

    public InteractionInputState(bool isInteractPressed, bool isInteractDenied)
    {
        IsInteractPressed = isInteractPressed;
        IsInteractDenied = isInteractDenied;
    }

    public static InteractionInputState Empty => new(false, false);
}