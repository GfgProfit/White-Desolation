public struct InteractionHoverInfo
{
    public string InteractionText;
    public string TimeText;
    public string TemperatureText;
    public string InfoText;

    public readonly bool HasInteractionText => !string.IsNullOrWhiteSpace(InteractionText);
    public readonly bool HasTimeText => !string.IsNullOrWhiteSpace(TimeText);
    public readonly bool HasTemperatureText => !string.IsNullOrWhiteSpace(TemperatureText);
    public readonly bool HasInfoText => !string.IsNullOrWhiteSpace(InfoText);

    public readonly bool HasExtraText => HasTimeText || HasTemperatureText || HasInfoText;
    public readonly bool HasAnyText => HasInteractionText || HasExtraText;

    public static InteractionHoverInfo Empty => new();

    public static InteractionHoverInfo Simple(string interactionText)
    {
        return new InteractionHoverInfo
        {
            InteractionText = interactionText
        };
    }
}