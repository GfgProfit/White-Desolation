public static class InventoryItemDetailsPresenter
{
    public static InventoryItemDetailsViewModel Build(
        InventorySlot slot,
        ItemUseContext useContext,
        bool isUsingItem)
    {
        bool hasSelection = slot != null && !slot.IsEmpty && slot.Item != null;

        if (!hasSelection)
        {
            return InventoryItemDetailsViewModel.NoSelection();
        }

        bool canUse = ItemUseService.CanUseSlot(useContext, slot);
        bool canDrop = !isUsingItem;

        return new InventoryItemDetailsViewModel(
            slot,
            true,
            canUse,
            canDrop,
            slot.Item.Icon != null,
            slot.Item.Icon,
            slot.Item.DisplayName,
            slot.Item.Description,
            InventoryDisplayFormatter.FormatPrimaryValue(slot),
            InventoryDisplayFormatter.FormatPrimaryActionLabel(slot),
            BuildDurabilityRow(slot),
            BuildWeightRow(slot),
            BuildCaloriesRow(slot),
            BuildHydrationRow(slot));
    }

    private static InventoryItemStatRowViewModel BuildDurabilityRow(InventorySlot slot)
    {
        bool isVisible = InventoryDisplayFormatter.TryGetDurabilityText(slot, out string text);
        return new InventoryItemStatRowViewModel(isVisible, text);
    }

    private static InventoryItemStatRowViewModel BuildWeightRow(InventorySlot slot)
    {
        bool isVisible = InventoryDisplayFormatter.TryGetWeightText(slot, out string text);
        return new InventoryItemStatRowViewModel(isVisible, text);
    }

    private static InventoryItemStatRowViewModel BuildCaloriesRow(InventorySlot slot)
    {
        bool isVisible = InventoryDisplayFormatter.TryGetCaloriesText(slot, out string text);
        return new InventoryItemStatRowViewModel(isVisible, text);
    }

    private static InventoryItemStatRowViewModel BuildHydrationRow(InventorySlot slot)
    {
        bool isVisible = InventoryDisplayFormatter.TryGetHydrationText(slot, out string text);
        return new InventoryItemStatRowViewModel(isVisible, text);
    }
}