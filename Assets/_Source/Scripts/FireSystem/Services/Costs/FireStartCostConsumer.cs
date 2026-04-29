using System.Collections.Generic;

public static class FireStartCostConsumer
{
    public static bool TryPay(InventoryController inventory, FireStartCost cost)
    {
        if (inventory == null || cost == null)
        {
            return false;
        }

        if (cost.IsEmpty)
        {
            return true;
        }

        if (!FireStartCostValidator.CanPay(inventory, cost))
        {
            return false;
        }

        IReadOnlyList<FireStartCostEntry> entries = cost.Entries;

        for (int i = 0; i < entries.Count; i++)
        {
            FireStartCostEntry entry = entries[i];

            if (!TryPayEntry(inventory, entry))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryPayEntry(InventoryController inventory, FireStartCostEntry entry)
    {
        if (inventory == null || entry == null || entry.Item == null)
        {
            return false;
        }

        return entry.Type switch
        {
            FireStartCostType.ItemCount => inventory.TryRemoveItem(entry.Item, entry.Count),
            FireStartCostType.CustomAmount => inventory.TryConsumeCustomAmountAcrossSlots(entry.Item, entry.Amount),
            FireStartCostType.Durability => inventory.TryConsumeDurabilityFromFirstMatchingItem(entry.Item, entry.DurabilityCost),
            _ => false,
        };
    }
}
