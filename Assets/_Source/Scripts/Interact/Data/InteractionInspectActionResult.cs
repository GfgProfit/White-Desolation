public readonly struct InteractionInspectActionResult
{
    public readonly bool ShouldClose;
    public readonly bool IsConfirmed;

    public InteractionInspectActionResult(bool shouldClose, bool isConfirmed)
    {
        ShouldClose = shouldClose;
        IsConfirmed = isConfirmed;
    }

    public static InteractionInspectActionResult None => new(false, false);
    public static InteractionInspectActionResult Close => new(true, false);
    public static InteractionInspectActionResult Confirmed => new(true, true);
}