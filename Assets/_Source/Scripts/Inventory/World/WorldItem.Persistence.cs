using UnityEngine;

public partial class WorldItem
{
    public void CaptureState(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(SaveId))
        {
            Debug.LogWarning($"{DebugPrefix} Cannot save WorldItem without SaveId: {name}");
            return;
        }

        WorldItemSaveDataCollection.RemoveBySaveId(saveData.WorldItems, SaveId);

        saveData.WorldItems.Add(new WorldItemSaveData
        {
            SaveId = SaveId,
            PickedUp = _pickedUp,
            ItemId = _itemData != null ? _itemData.Id : string.Empty,
            Count = _count,
            OverrideCurrentAmount = _overrideCurrentAmount,
            CurrentAmount = _currentAmount,
            OverrideCurrentDurability = _overrideCurrentDurability,
            CurrentDurability = _currentDurability,
            Position = new SerializableVector3(transform.position),
            Rotation = new SerializableQuaternion(transform.rotation)
        });
    }

    public void RestoreState(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null || saveData.WorldItems == null)
        {
            return;
        }

        WorldItemSaveData itemSaveData = WorldItemSaveDataCollection.FindBySaveId(saveData.WorldItems, SaveId);

        if (itemSaveData == null)
        {
            return;
        }

        if (context != null && context.ItemDatabase != null && !string.IsNullOrWhiteSpace(itemSaveData.ItemId) && context.ItemDatabase.TryGetItem(itemSaveData.ItemId, out ItemData restoredItem))
        {
            _itemData = restoredItem;
        }

        _count = Mathf.Max(1, itemSaveData.Count);

        _overrideCurrentAmount = itemSaveData.OverrideCurrentAmount;
        _currentAmount = itemSaveData.CurrentAmount;

        _overrideCurrentDurability = itemSaveData.OverrideCurrentDurability;
        _currentDurability = itemSaveData.CurrentDurability;

        transform.SetPositionAndRotation(itemSaveData.Position.ToVector3(), itemSaveData.Rotation.ToQuaternion());

        SetPickedUp(itemSaveData.PickedUp);
    }
}