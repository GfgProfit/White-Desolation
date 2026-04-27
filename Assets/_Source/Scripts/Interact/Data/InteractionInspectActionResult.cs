public readonly struct InteractionInspectActionResult
{
    public readonly bool ShouldClose;
    public readonly bool ShouldClearHover;

    public InteractionInspectActionResult(bool shouldClose, bool shouldClearHover)
    {
        ShouldClose = shouldClose;
        ShouldClearHover = shouldClearHover;
    }

    public static InteractionInspectActionResult None => new(false, false);
    public static InteractionInspectActionResult Close => new(true, false);
    public static InteractionInspectActionResult CloseAndClearHover => new(true, true);
}