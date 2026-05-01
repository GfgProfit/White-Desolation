using System.Collections.Generic;

public sealed partial class CrateContainer
{
    public void CaptureState(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        saveData.Crates ??= new List<CrateSaveData>();

        CrateSaveDataCollection.RemoveBySaveId(saveData.Crates, SaveId);

        CrateSaveData crateSaveData = new()
        {
            SaveId = SaveId,
            LootGenerated = _lootGenerated,
            Searched = _searched
        };

        for (int i = 0; i < _items.Count; i++)
        {
            if (InventorySlotSaveDataMapper.TryCreateSaveData(_items[i], out InventorySlotSaveData slotSaveData))
            {
                crateSaveData.Items.Add(slotSaveData);
            }
        }

        saveData.Crates.Add(crateSaveData);
    }

    public void RestoreState(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null || saveData.Crates == null)
        {
            return;
        }

        CrateSaveData crateSaveData = CrateSaveDataCollection.FindBySaveId(saveData.Crates, SaveId);

        if (crateSaveData == null)
        {
            return;
        }

        _lootGenerated = crateSaveData.LootGenerated;
        _searched = crateSaveData.Searched;
        _items.Clear();

        if (crateSaveData.Items != null)
        {
            for (int i = 0; i < crateSaveData.Items.Count; i++)
            {
                if (InventorySlotSaveDataMapper.TryCreateSlot(crateSaveData.Items[i], context, out InventorySlot slot))
                {
                    _items.Add(slot);
                }
            }
        }

        NotifyChanged();
    }
}
