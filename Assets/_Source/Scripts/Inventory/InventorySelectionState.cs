using System.Collections.Generic;

public sealed class InventorySelectionState
{
    public int SelectedSlotIndex { get; private set; } = -1;

    public bool HasSelection => SelectedSlotIndex >= 0;

    public void SelectSlot(int slotIndex)
    {
        SelectedSlotIndex = slotIndex;
    }

    public void Clear()
    {
        SelectedSlotIndex = -1;
    }

    public void ValidateForVisibleEntries(List<InventoryViewEntry> visibleEntries)
    {
        if (visibleEntries == null || visibleEntries.Count == 0)
        {
            Clear();
            return;
        }

        if (HasSelection && ContainsSlotIndex(visibleEntries, SelectedSlotIndex))
        {
            return;
        }

        SelectedSlotIndex = visibleEntries[0].SlotIndex;
    }

    public bool IsSelected(int slotIndex)
    {
        return SelectedSlotIndex == slotIndex;
    }

    private static bool ContainsSlotIndex(List<InventoryViewEntry> visibleEntries, int slotIndex)
    {
        for (int i = 0; i < visibleEntries.Count; i++)
        {
            if (visibleEntries[i].SlotIndex == slotIndex)
            {
                return true;
            }
        }

        return false;
    }
}