using System.Collections.Generic;

public static class CrateInventoryAddService
{
    private const float ZeroTolerance = 0.0001f;

    public static bool TryAddItem(
        List<InventorySlot> slots,
        float maxWeightKg,
        ItemData item,
        int count,
        float? currentAmountOverride = null,
        float? currentDurabilityOverride = null,
        float? currentHydrationOverride = null,
        float? currentCaloriesOverride = null)
    {
        if (slots == null)
        {
            return false;
        }

        if (!InventoryAddCapacityPolicy.CanAddItem(item, count, currentAmountOverride))
        {
            return false;
        }

        float incomingWeight = InventoryWeightCalculator.CalculateIncomingWeightKg(item, count, currentAmountOverride, currentHydrationOverride, currentCaloriesOverride);
        float currentWeight = InventoryWeightCalculator.CalculateTotalWeightKg(slots);

        if (!InventoryCapacityPolicy.CanAcceptWeight(currentWeight, maxWeightKg, incomingWeight))
        {
            return false;
        }

        if (item.UsesCustomAmount)
        {
            return InventoryCustomAmountAddService.TryAddCustomAmountItem(slots, item, count, currentAmountOverride, currentDurabilityOverride, ZeroTolerance);
        }

        if (InventoryConsumableInstancePolicy.RequiresDedicatedInstance(item))
        {
            return InventorySeparateSlotAddService.TryAddSeparateSlotItems(slots, item, count, currentDurabilityOverride, currentAmountOverride, currentHydrationOverride, currentCaloriesOverride);
        }

        if (item.IsStackable)
        {
            return InventoryStackableAddService.TryAddStackableItems(slots, item, count, currentDurabilityOverride, currentAmountOverride);
        }

        return InventorySeparateSlotAddService.TryAddSeparateSlotItems(slots, item, count, currentDurabilityOverride, currentAmountOverride, currentHydrationOverride, currentCaloriesOverride);
    }
}
