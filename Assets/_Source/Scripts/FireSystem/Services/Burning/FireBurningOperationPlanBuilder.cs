public static class FireBurningOperationPlanBuilder
{
    public static FireBurningOperationPlan Build(FireBurningOperationTab tab, FireBurningWaterMode waterMode, FireSourceInteractable source, InventoryController inventory, FireBurningOperationList list, int selectedIndex, float selectedWaterAmount, ItemData meltedWaterItem, ItemData boiledWaterItem, FireBurningOperationSettings settings)
    {
        if (tab == FireBurningOperationTab.AddFuel)
        {
            return BuildAddFuelPlan(source, inventory, list, selectedIndex);
        }

        if (tab == FireBurningOperationTab.Cooking)
        {
            return BuildCookingPlan(source, inventory, list, selectedIndex);
        }

        if (waterMode == FireBurningWaterMode.MeltSnow)
        {
            return BuildMeltSnowPlan(source, inventory, selectedWaterAmount, meltedWaterItem, settings);
        }

        return BuildBoilWaterPlan(source, inventory, selectedWaterAmount, meltedWaterItem, boiledWaterItem, settings);
    }

    private static FireBurningOperationPlan BuildAddFuelPlan(FireSourceInteractable source, InventoryController inventory, FireBurningOperationList list, int selectedIndex)
    {
        if (!TryGetSelectedSlot(inventory, list, selectedIndex, out int slotIndex, out InventorySlot slot))
        {
            return FireBurningOperationPlan.Create(FireBurningOperationType.AddFuel, false);
        }

        ItemData item = slot.Item;
        bool canAdd = source != null && source.IsBurning && item != null && item.BurnMinutes > 0f;

        return FireBurningOperationPlan.Create(FireBurningOperationType.AddFuel, canAdd, slotIndex: slotIndex, sourceItem: item);
    }

    private static FireBurningOperationPlan BuildCookingPlan(FireSourceInteractable source, InventoryController inventory, FireBurningOperationList list, int selectedIndex)
    {
        if (!TryGetSelectedSlot(inventory, list, selectedIndex, out int slotIndex, out InventorySlot slot))
        {
            return FireBurningOperationPlan.Create(FireBurningOperationType.Cook, false);
        }

        ItemData item = slot.Item;
        ItemData result = item.CookedResult;
        float gameMinutes = item.CookGameMinutes;
        bool hasEnoughFire = source != null && source.HasEnoughBurnTime(gameMinutes);
        bool canAddResult = inventory != null && result != null && inventory.CanAddItem(result, 1);
        bool canCook = item.CanBeCooked && hasEnoughFire && canAddResult;

        return FireBurningOperationPlan.Create(FireBurningOperationType.Cook, canCook, gameMinutes, slotIndex, item, result, resultDurabilityOverride: slot.CurrentDurability);
    }

    private static FireBurningOperationPlan BuildMeltSnowPlan(FireSourceInteractable source, InventoryController inventory, float amount, ItemData meltedWaterItem, FireBurningOperationSettings settings)
    {
        float gameMinutes = settings.GetGameMinutes(FireBurningWaterMode.MeltSnow, amount);
        bool waterItemValid = IsValidCustomAmountItem(meltedWaterItem);
        bool hasEnoughFire = source != null && source.HasEnoughBurnTime(gameMinutes);
        bool canAddResult = inventory != null && waterItemValid && inventory.CanAddItem(meltedWaterItem, 1, amount);
        bool canMelt = amount > 0f && waterItemValid && hasEnoughFire && canAddResult;

        return FireBurningOperationPlan.Create(FireBurningOperationType.MeltSnow, canMelt, gameMinutes, resultItem: meltedWaterItem, amount: amount);
    }

    private static FireBurningOperationPlan BuildBoilWaterPlan(FireSourceInteractable source, InventoryController inventory, float amount, ItemData meltedWaterItem, ItemData boiledWaterItem, FireBurningOperationSettings settings)
    {
        float gameMinutes = settings.GetGameMinutes(FireBurningWaterMode.BoilWater, amount);
        bool inputItemValid = IsValidCustomAmountItem(meltedWaterItem);
        bool outputItemValid = IsValidCustomAmountItem(boiledWaterItem);
        bool hasEnoughWater = inventory != null && inputItemValid && inventory.HasCustomAmount(meltedWaterItem, amount);
        bool hasEnoughFire = source != null && source.HasEnoughBurnTime(gameMinutes);
        bool canAddResult = inventory != null && outputItemValid && inventory.CanAddItem(boiledWaterItem, 1, amount);
        bool canBoil = amount > 0f && inputItemValid && outputItemValid && hasEnoughWater && hasEnoughFire && canAddResult;

        return FireBurningOperationPlan.Create(FireBurningOperationType.BoilWater, canBoil, gameMinutes, sourceItem: meltedWaterItem, resultItem: boiledWaterItem, amount: amount);
    }

    private static bool TryGetSelectedSlot(InventoryController inventory, FireBurningOperationList list, int selectedIndex, out int slotIndex, out InventorySlot slot)
    {
        slotIndex = -1;
        slot = null;

        if (inventory == null || list == null)
        {
            return false;
        }

        if (!list.TryGetSlotIndex(selectedIndex, out slotIndex))
        {
            return false;
        }

        slot = inventory.GetSlotAt(slotIndex);

        return slot != null && !slot.IsEmpty && slot.Item != null;
    }

    private static bool IsValidCustomAmountItem(ItemData item)
    {
        return item != null && item.UsesCustomAmount;
    }
}
