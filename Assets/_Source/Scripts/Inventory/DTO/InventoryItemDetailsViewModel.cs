using UnityEngine;

public readonly struct InventoryItemDetailsViewModel
{
    public InventorySlot SourceSlot { get; }

    public bool HasSelection { get; }
    public bool CanUse { get; }
    public bool CanDrop { get; }

    public bool IconEnabled { get; }
    public Sprite Icon { get; }

    public string NameText { get; }
    public string DescriptionText { get; }
    public string CountText { get; }
    public string PrimaryActionLabel { get; }

    public InventoryItemStatRowViewModel Durability { get; }
    public InventoryItemStatRowViewModel Weight { get; }
    public InventoryItemStatRowViewModel Calories { get; }
    public InventoryItemStatRowViewModel Hydration { get; }

    public InventoryItemDetailsViewModel(
        InventorySlot sourceSlot,
        bool hasSelection,
        bool canUse,
        bool canDrop,
        bool iconEnabled,
        Sprite icon,
        string nameText,
        string descriptionText,
        string countText,
        string primaryActionLabel,
        InventoryItemStatRowViewModel durability,
        InventoryItemStatRowViewModel weight,
        InventoryItemStatRowViewModel calories,
        InventoryItemStatRowViewModel hydration)
    {
        SourceSlot = sourceSlot;

        HasSelection = hasSelection;
        CanUse = canUse;
        CanDrop = canDrop;

        IconEnabled = iconEnabled;
        Icon = icon;

        NameText = nameText ?? string.Empty;
        DescriptionText = descriptionText ?? string.Empty;
        CountText = countText ?? string.Empty;
        PrimaryActionLabel = primaryActionLabel ?? "Использовать";

        Durability = durability;
        Weight = weight;
        Calories = calories;
        Hydration = hydration;
    }

    public static InventoryItemDetailsViewModel NoSelection()
    {
        return new InventoryItemDetailsViewModel(
            null,
            false,
            false,
            false,
            false,
            null,
            "Не выбран предмет.",
            string.Empty,
            string.Empty,
            "Использовать",
            InventoryItemStatRowViewModel.Hidden,
            InventoryItemStatRowViewModel.Hidden,
            InventoryItemStatRowViewModel.Hidden,
            InventoryItemStatRowViewModel.Hidden);
    }
}