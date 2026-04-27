public partial class InventoryController
{
    public void CaptureState(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.InventorySlots.Clear();

        for (int i = 0; i < _items.Count; i++)
        {
            InventorySlot slot = _items[i];

            if (!InventorySlotSaveDataMapper.TryCreateSaveData(slot, out InventorySlotSaveData slotSaveData))
            {
                continue;
            }

            saveData.InventorySlots.Add(slotSaveData);
        }
    }

    public void RestoreState(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null)
        {
            return;
        }

        _items.Clear();

        if (saveData.InventorySlots == null)
        {
            NotifyChanged();
            return;
        }

        for (int i = 0; i < saveData.InventorySlots.Count; i++)
        {
            InventorySlotSaveData slotData = saveData.InventorySlots[i];

            if (!InventorySlotSaveDataMapper.TryCreateSlot(slotData, context, out InventorySlot slot))
            {
                continue;
            }

            _items.Add(slot);
        }

        NotifyChanged();
    }
}