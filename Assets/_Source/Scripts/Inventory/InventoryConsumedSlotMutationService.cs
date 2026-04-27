using System.Collections.Generic;

public static class InventoryConsumedSlotMutationService
{
    public static void RemoveOrReplaceIfDepleted(List<InventorySlot> slots, int slotIndex, InventorySlot slot, ItemData replaceWhenDepleted)
    {
        if (slots == null)
        {
            return;
        }

        if (slot == null)
        {
            return;
        }

        if (!InventorySlotConsumePolicy.ShouldRemoveSlotAfterConsume(slot))
        {
            return;
        }

        if (replaceWhenDepleted != null)
        {
            InventorySlotReplacementService.TryReplaceSlotItem(slot, replaceWhenDepleted);

            return;
        }

        if (!InventorySlotQuery.IsIndexInRange(slots, slotIndex))
        {
            return;
        }

        slots.RemoveAt(slotIndex);
    }
}