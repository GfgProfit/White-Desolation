using UnityEngine;

public readonly struct InteractionInspectInfo
{
    public readonly Sprite Icon;
    public readonly string Name;
    public readonly string Description;

    public readonly string DurabilityText;
    public readonly bool HasDurabilityVisual;
    public readonly Color DurabilityColor;

    public readonly string WeightText;

    public bool HasName => !string.IsNullOrWhiteSpace(Name);
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
    public bool HasDurabilityText => !string.IsNullOrWhiteSpace(DurabilityText);
    public bool HasWeightText => !string.IsNullOrWhiteSpace(WeightText);

    public InteractionInspectInfo(Sprite icon, string name, string description, string durabilityText, bool hasDurabilityVisual, Color durabilityColor, string weightText)
    {
        Icon = icon;
        Name = name;
        Description = description;
        DurabilityText = durabilityText;
        HasDurabilityVisual = hasDurabilityVisual;
        DurabilityColor = durabilityColor;
        WeightText = weightText;
    }

    public static InteractionInspectInfo Empty => new(null, string.Empty, string.Empty, string.Empty, false, Color.white, string.Empty);
}