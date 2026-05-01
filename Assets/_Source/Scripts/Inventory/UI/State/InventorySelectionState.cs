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
        ValidateForVisibleEntries(visibleEntries, 0);
    }

    public void ValidateForVisibleEntries(List<InventoryViewEntry> visibleEntries, int preferredVisibleIndex)
    {
        ValidateForVisibleEntries(visibleEntries, preferredVisibleIndex, null);
    }

    public void ValidateForVisibleEntries(List<InventoryViewEntry> visibleEntries, int preferredVisibleIndex, InventorySlot preferredSlot)
    {
        if (visibleEntries == null || visibleEntries.Count == 0)
        {
            Clear();
            return;
        }

        if (preferredSlot != null && TrySelectSlotReference(visibleEntries, preferredSlot))
        {
            return;
        }

        if (preferredSlot == null && HasSelection && ContainsSlotIndex(visibleEntries, SelectedSlotIndex))
        {
            return;
        }

        int fallbackIndex = ClampVisibleIndex(preferredVisibleIndex, visibleEntries.Count);
        SelectedSlotIndex = visibleEntries[fallbackIndex].SlotIndex;
    }

    public bool IsSelected(int slotIndex)
    {
        return SelectedSlotIndex == slotIndex;
    }

    public int GetVisibleIndex(List<InventoryViewEntry> visibleEntries)
    {
        if (visibleEntries == null || !HasSelection)
        {
            return 0;
        }

        for (int i = 0; i < visibleEntries.Count; i++)
        {
            if (visibleEntries[i].SlotIndex == SelectedSlotIndex)
            {
                return i;
            }
        }

        return 0;
    }

    public InventorySlot GetSelectedSlot(List<InventoryViewEntry> visibleEntries)
    {
        if (visibleEntries == null || !HasSelection)
        {
            return null;
        }

        for (int i = 0; i < visibleEntries.Count; i++)
        {
            InventoryViewEntry entry = visibleEntries[i];

            if (entry.SlotIndex == SelectedSlotIndex)
            {
                return entry.Slot;
            }
        }

        return null;
    }

    private bool TrySelectSlotReference(List<InventoryViewEntry> visibleEntries, InventorySlot slot)
    {
        for (int i = 0; i < visibleEntries.Count; i++)
        {
            if (ReferenceEquals(visibleEntries[i].Slot, slot))
            {
                SelectedSlotIndex = visibleEntries[i].SlotIndex;
                return true;
            }
        }

        return false;
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

    private static int ClampVisibleIndex(int index, int count)
    {
        if (count <= 0)
        {
            return -1;
        }

        if (index < 0)
        {
            return 0;
        }

        if (index >= count)
        {
            return count - 1;
        }

        return index;
    }
}
