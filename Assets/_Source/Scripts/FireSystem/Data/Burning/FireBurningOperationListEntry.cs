using UnityEngine;

public readonly struct FireBurningOperationListEntry
{
    public readonly Sprite Icon;
    public readonly string Name;
    public readonly bool Interactable;
    public readonly bool SupportsAmountControls;

    public FireBurningOperationListEntry(Sprite icon, string name, bool interactable, bool supportsAmountControls = false)
    {
        Icon = icon;
        Name = name;
        Interactable = interactable;
        SupportsAmountControls = supportsAmountControls;
    }
}
