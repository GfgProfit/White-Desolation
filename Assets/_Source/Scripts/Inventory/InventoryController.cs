using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private const string DebugPrefix = "<color=cyan>[Inventory]</color>";

    private readonly List<InventorySlot> _items = new();

    public IReadOnlyList<InventorySlot> Items => _items;

    public int SlotCount => _items.Count;

    public event Action OnInventoryChanged;

    public bool TryAddItem(ItemData itemData, int count)
    {
        if (itemData == null)
        {
            Debug.LogWarning($"{DebugPrefix} Cannot add item: ItemData is null.");
            return false;
        }

        if (count <= 0)
        {
            Debug.LogWarning($"{DebugPrefix} Cannot add item {itemData.DisplayName}: count must be > 0.");
            return false;
        }

        int remaining = count;

        if (itemData.IsStackable)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                InventorySlot slot = _items[i];

                if (slot == null || slot.IsEmpty)
                    continue;

                if (!AreSameItem(slot.Item, itemData))
                    continue;

                if (slot.IsFull)
                    continue;

                int freeSpace = slot.MaxStack - slot.Count;
                int amountToAdd = Mathf.Min(freeSpace, remaining);

                slot.Count += amountToAdd;
                remaining -= amountToAdd;

                if (remaining <= 0)
                {
                    NotifyChanged();
                    Debug.Log($"{DebugPrefix} Added {count}x {itemData.DisplayName}.");
                    return true;
                }
            }
        }

        while (remaining > 0)
        {
            int amountForNewSlot = itemData.IsStackable
                ? Mathf.Min(itemData.MaxStack, remaining)
                : 1;

            _items.Add(new InventorySlot
            {
                Item = itemData,
                Count = amountForNewSlot
            });

            remaining -= amountForNewSlot;
        }

        NotifyChanged();
        Debug.Log($"{DebugPrefix} Added {count}x {itemData.DisplayName}. {Items.Count}");
        return true;
    }

    public bool TryRemoveItem(ItemData itemData, int count)
    {
        if (itemData == null)
        {
            Debug.LogWarning($"{DebugPrefix} Cannot remove item: ItemData is null.");
            return false;
        }

        if (count <= 0)
        {
            Debug.LogWarning($"{DebugPrefix} Cannot remove item {itemData.DisplayName}: count must be > 0.");
            return false;
        }

        int totalCount = GetTotalCount(itemData);
        if (totalCount < count)
        {
            Debug.LogWarning($"{DebugPrefix} Not enough {itemData.DisplayName}. Need {count}, have {totalCount}.");
            return false;
        }

        int remainingToRemove = count;

        for (int i = _items.Count - 1; i >= 0; i--)
        {
            InventorySlot slot = _items[i];

            if (slot == null || slot.IsEmpty)
                continue;

            if (!AreSameItem(slot.Item, itemData))
                continue;

            int amountToRemove = Mathf.Min(slot.Count, remainingToRemove);
            slot.Count -= amountToRemove;
            remainingToRemove -= amountToRemove;

            if (slot.Count <= 0)
            {
                _items.RemoveAt(i);
            }

            if (remainingToRemove <= 0)
            {
                NotifyChanged();
                Debug.Log($"{DebugPrefix} Removed {count}x {itemData.DisplayName}.");
                return true;
            }
        }

        NotifyChanged();
        return true;
    }

    public InventorySlot GetSlotAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _items.Count)
            return null;

        return _items[slotIndex];
    }

    public bool TryRemoveFromSlot(int slotIndex, int count)
    {
        if (slotIndex < 0 || slotIndex >= _items.Count)
        {
            Debug.LogWarning($"{DebugPrefix} Invalid slot index: {slotIndex}.");
            return false;
        }

        if (count <= 0)
        {
            Debug.LogWarning($"{DebugPrefix} Remove count must be > 0.");
            return false;
        }

        InventorySlot slot = _items[slotIndex];
        if (slot == null || slot.IsEmpty)
        {
            Debug.LogWarning($"{DebugPrefix} Slot {slotIndex} is empty.");
            return false;
        }

        int amountToRemove = Mathf.Min(count, slot.Count);
        slot.Count -= amountToRemove;

        if (slot.Count <= 0)
        {
            _items.RemoveAt(slotIndex);
        }

        NotifyChanged();
        return true;
    }

    public int GetTotalCount(ItemData itemData)
    {
        if (itemData == null)
            return 0;

        int total = 0;

        for (int i = 0; i < _items.Count; i++)
        {
            InventorySlot slot = _items[i];

            if (slot == null || slot.IsEmpty)
                continue;

            if (!AreSameItem(slot.Item, itemData))
                continue;

            total += slot.Count;
        }

        return total;
    }

    public bool Contains(ItemData itemData, int count = 1)
    {
        return GetTotalCount(itemData) >= count;
    }

    public void ClearInventory()
    {
        _items.Clear();
        NotifyChanged();
    }

    private static bool AreSameItem(ItemData a, ItemData b)
    {
        if (a == null || b == null)
            return false;

        return a.Id == b.Id;
    }

    private void NotifyChanged()
    {
        OnInventoryChanged?.Invoke();
    }
}