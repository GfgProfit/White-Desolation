public static class FireBurningOperationListBuilder
{
    public static void Rebuild(FireBurningOperationList result, FireBurningOperationTab tab, InventoryController inventory, FireStartingConfig config, ItemData meltedWaterItem, ItemData boiledWaterItem)
    {
        if (result == null)
        {
            return;
        }

        result.Clear();

        if (tab == FireBurningOperationTab.AddFuel)
        {
            BuildFuelEntries(result, inventory, config);
            return;
        }

        if (tab == FireBurningOperationTab.Cooking)
        {
            BuildCookingEntries(result, inventory);
            return;
        }

        BuildWaterEntries(result, inventory, meltedWaterItem, boiledWaterItem);
    }

    private static void BuildFuelEntries(FireBurningOperationList result, InventoryController inventory, FireStartingConfig config)
    {
        if (inventory == null)
        {
            return;
        }

        for (int i = 0; i < inventory.Items.Count; i++)
        {
            InventorySlot slot = inventory.Items[i];

            if (slot == null || slot.IsEmpty || slot.Item == null)
            {
                continue;
            }

            if (!IsFuelItem(slot.Item, config))
            {
                continue;
            }

            result.Add(new FireBurningOperationListEntry(slot.Item.Icon, FireBurningDisplayFormatter.BuildSlotDisplayName(slot), true), i);
        }
    }

    private static void BuildCookingEntries(FireBurningOperationList result, InventoryController inventory)
    {
        if (inventory == null)
        {
            return;
        }

        for (int i = 0; i < inventory.Items.Count; i++)
        {
            InventorySlot slot = inventory.Items[i];

            if (slot == null || slot.IsEmpty || slot.Item == null || !slot.Item.CanBeCooked)
            {
                continue;
            }

            result.Add(new FireBurningOperationListEntry(slot.Item.Icon, FireBurningDisplayFormatter.BuildSlotDisplayName(slot), true), i);
        }
    }

    private static void BuildWaterEntries(FireBurningOperationList result, InventoryController inventory, ItemData meltedWaterItem, ItemData boiledWaterItem)
    {
        bool canBoilWater = inventory != null && meltedWaterItem != null && inventory.GetTotalAmount(meltedWaterItem) > 0f;

        result.Add(new FireBurningOperationListEntry(meltedWaterItem != null ? meltedWaterItem.Icon : null, "Топить снег", true, true));
        result.Add(new FireBurningOperationListEntry(boiledWaterItem != null ? boiledWaterItem.Icon : null, "Кипятить воду", canBoilWater, true));
    }

    private static bool IsFuelItem(ItemData item, FireStartingConfig config)
    {
        if (item == null || item.BurnMinutes <= 0f)
        {
            return false;
        }

        if (config == null || config.Fuels == null || config.Fuels.Length == 0)
        {
            return true;
        }

        return ContainsItem(config.Fuels, item);
    }

    private static bool ContainsItem(ItemData[] items, ItemData item)
    {
        if (items == null || item == null)
        {
            return false;
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (ItemDataComparer.AreSame(items[i], item))
            {
                return true;
            }
        }

        return false;
    }
}
