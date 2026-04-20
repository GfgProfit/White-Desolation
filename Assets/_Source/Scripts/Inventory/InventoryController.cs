using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private const string DebugPrefix = "[Inventory]";
    private const float ZeroTolerance = 0.0001f;

    [Header("Weight Limit")]
    [SerializeField, Min(0f)] private float _maxCarryWeightKg = 30f;

    private readonly List<InventorySlot> _items = new();

    public IReadOnlyList<InventorySlot> Items => _items;
    public int SlotCount => _items.Count;

    public float MaxCarryWeightKg => _maxCarryWeightKg;
    public float CurrentCarryWeightKg => GetCurrentTotalWeightKg();

    public event Action OnInventoryChanged;

    public bool TryAddItem(
        ItemData itemData,
        int count,
        float? currentAmountOverride = null,
        float? currentDurabilityOverride = null)
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

        if (itemData.UsesCustomAmount)
        {
            bool addedCustomAmount = TryAddCustomAmountItem(itemData, count, currentAmountOverride, currentDurabilityOverride);
            if (addedCustomAmount)
            {
                NotifyChanged();
                Debug.Log($"{DebugPrefix} Added custom-amount item {itemData.DisplayName} x{count}.");
            }

            return addedCustomAmount;
        }

        if (RequiresDedicatedConsumableInstance(itemData))
        {
            for (int i = 0; i < count; i++)
            {
                InventorySlot newSlot = new();
                newSlot.Initialize(itemData, 1, currentDurabilityOverride, currentAmountOverride);
                _items.Add(newSlot);
            }

            NotifyChanged();
            Debug.Log($"{DebugPrefix} Added instance consumable {itemData.DisplayName} x{count}.");
            return true;
        }

        if (itemData.IsStackable)
        {
            int remaining = count;

            for (int i = 0; i < _items.Count; i++)
            {
                InventorySlot slot = _items[i];
                if (slot == null || slot.IsEmpty)
                    continue;

                if (!CanMergeIntoStack(slot, itemData, currentDurabilityOverride, currentAmountOverride))
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

            while (remaining > 0)
            {
                int amountForNewSlot = Mathf.Min(itemData.MaxStack, remaining);

                InventorySlot newSlot = new();
                newSlot.Initialize(itemData, amountForNewSlot, currentDurabilityOverride, currentAmountOverride);

                _items.Add(newSlot);
                remaining -= amountForNewSlot;
            }

            NotifyChanged();
            Debug.Log($"{DebugPrefix} Added {count}x {itemData.DisplayName}.");
            return true;
        }

        for (int i = 0; i < count; i++)
        {
            InventorySlot newSlot = new();
            newSlot.Initialize(itemData, 1, currentDurabilityOverride, currentAmountOverride);

            _items.Add(newSlot);
        }

        NotifyChanged();
        Debug.Log($"{DebugPrefix} Added {count}x {itemData.DisplayName}.");
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
                _items.RemoveAt(i);

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

    public bool TryReplaceSlotItem(int slotIndex, ItemData newItemData)
    {
        if (slotIndex < 0 || slotIndex >= _items.Count)
        {
            Debug.LogWarning($"{DebugPrefix} Invalid slot index: {slotIndex}.");
            return false;
        }

        if (newItemData == null)
        {
            Debug.LogWarning($"{DebugPrefix} Cannot replace slot {slotIndex}: new ItemData is null.");
            return false;
        }

        InventorySlot slot = _items[slotIndex];
        if (slot == null || slot.IsEmpty)
        {
            Debug.LogWarning($"{DebugPrefix} Slot {slotIndex} is empty.");
            return false;
        }

        int count = Mathf.Max(1, slot.Count);
        slot.Initialize(newItemData, count);

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
            _items.RemoveAt(slotIndex);

        NotifyChanged();
        return true;
    }

    public bool TryConsumeFromSlot(
        int slotIndex,
        float hydrationToConsume = 0f,
        float caloriesToConsume = 0f,
        float amountToConsume = 0f,
        ItemData replaceWhenDepleted = null)
    {
        if (slotIndex < 0 || slotIndex >= _items.Count)
        {
            Debug.LogWarning($"{DebugPrefix} Invalid slot index: {slotIndex}.");
            return false;
        }

        InventorySlot slot = _items[slotIndex];
        if (slot == null || slot.IsEmpty || slot.Item == null)
        {
            Debug.LogWarning($"{DebugPrefix} Slot {slotIndex} is empty.");
            return false;
        }

        if (!Mathf.Approximately(hydrationToConsume, 0f))
        {
            slot.CurrentHydration -= hydrationToConsume;
            if (Mathf.Abs(slot.CurrentHydration) <= ZeroTolerance)
                slot.CurrentHydration = 0f;
        }

        if (!Mathf.Approximately(caloriesToConsume, 0f))
        {
            slot.CurrentCalories -= caloriesToConsume;
            if (Mathf.Abs(slot.CurrentCalories) <= ZeroTolerance)
                slot.CurrentCalories = 0f;
        }

        if (!Mathf.Approximately(amountToConsume, 0f))
        {
            slot.CurrentAmount = Mathf.Max(0f, slot.CurrentAmount - amountToConsume);
        }

        if (ShouldRemoveSlotAfterConsume(slot))
        {
            if (replaceWhenDepleted != null)
            {
                int count = Mathf.Max(1, slot.Count);
                slot.Initialize(replaceWhenDepleted, count);
            }
            else
            {
                _items.RemoveAt(slotIndex);
            }
        }

        NotifyChanged();
        return true;
    }

    public bool ContainsUsableItem(ItemData itemData, int count = 1)
    {
        if (itemData == null || count <= 0)
            return false;

        int found = 0;

        for (int i = 0; i < _items.Count; i++)
        {
            InventorySlot slot = _items[i];
            if (slot == null || slot.IsEmpty || slot.Item == null)
                continue;

            if (!AreSameItem(slot.Item, itemData))
                continue;

            if (slot.HasDurability && slot.IsBroken)
                continue;

            found += slot.Count;

            if (found >= count)
                return true;
        }

        return false;
    }

    public bool TryConsumeDurabilityFromFirstMatchingItem(ItemData itemData, float durabilityCost)
    {
        if (itemData == null)
        {
            Debug.LogWarning($"{DebugPrefix} Cannot consume durability: ItemData is null.");
            return false;
        }

        if (durabilityCost <= ZeroTolerance)
            return Contains(itemData);

        for (int i = 0; i < _items.Count; i++)
        {
            InventorySlot slot = _items[i];
            if (slot == null || slot.IsEmpty || slot.Item == null)
                continue;

            if (!AreSameItem(slot.Item, itemData))
                continue;

            if (!slot.HasDurability)
            {
                return true;
            }

            if (slot.IsBroken)
                continue;

            slot.CurrentDurability = Mathf.Max(0f, slot.CurrentDurability - durabilityCost);

            if (slot.IsBroken)
            {
                Debug.Log($"{DebugPrefix} {slot.Item.DisplayName} broke.");
            }

            NotifyChanged();
            return true;
        }

        Debug.LogWarning($"{DebugPrefix} No usable tool found for {itemData.DisplayName}.");
        return false;
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

    public float GetTotalAmount(ItemData itemData)
    {
        if (itemData == null || !itemData.UsesCustomAmount)
            return 0f;

        float total = 0f;

        for (int i = 0; i < _items.Count; i++)
        {
            InventorySlot slot = _items[i];
            if (slot == null || slot.IsEmpty)
                continue;

            if (!AreSameItem(slot.Item, itemData))
                continue;

            if (!slot.HasAmount)
                continue;

            total += slot.CurrentAmount;
        }

        return total;
    }

    public float GetCurrentTotalWeightKg()
    {
        float totalWeight = 0f;

        for (int i = 0; i < _items.Count; i++)
        {
            InventorySlot slot = _items[i];
            if (slot == null || slot.IsEmpty)
                continue;

            totalWeight += InventoryWeightCalculator.GetSlotWeightKg(slot);
        }

        return totalWeight;
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

    private bool TryAddCustomAmountItem(
        ItemData itemData,
        int count,
        float? currentAmountOverride,
        float? currentDurabilityOverride)
    {
        float amountPerItem = currentAmountOverride ?? itemData.MaxAmount;

        if (amountPerItem <= ZeroTolerance)
        {
            Debug.LogWarning($"{DebugPrefix} Cannot add {itemData.DisplayName}: amount must be > 0.");
            return false;
        }

        for (int itemIndex = 0; itemIndex < count; itemIndex++)
        {
            float remainingAmountForItem = amountPerItem;

            while (remainingAmountForItem > ZeroTolerance)
            {
                float amountForSlot = Mathf.Min(itemData.MaxAmount, remainingAmountForItem);

                InventorySlot newSlot = new();
                newSlot.Initialize(itemData, 1, currentDurabilityOverride, amountForSlot);

                _items.Add(newSlot);
                remainingAmountForItem -= amountForSlot;
            }
        }

        return true;
    }

    private bool CanMergeIntoStack(
        InventorySlot slot,
        ItemData incomingItem,
        float? incomingDurabilityOverride,
        float? incomingAmountOverride)
    {
        if (slot == null || slot.IsEmpty || slot.Item == null || incomingItem == null)
            return false;

        if (!AreSameItem(slot.Item, incomingItem))
            return false;

        if (slot.UsesPerInstanceConsumableState || RequiresDedicatedConsumableInstance(incomingItem))
            return false;

        if (slot.Item.UsesCustomAmount || incomingItem.UsesCustomAmount)
        {
            float incomingAmount = Mathf.Clamp(
                incomingAmountOverride ?? incomingItem.MaxAmount,
                0f,
                incomingItem.MaxAmount);

            return Mathf.Approximately(slot.CurrentAmount, incomingAmount);
        }

        if (slot.Item.UsesDurability && !slot.Item.IsUnbreakable)
        {
            float incomingDurability = Mathf.Clamp(
                incomingDurabilityOverride ?? incomingItem.MaxDurability,
                0f,
                incomingItem.MaxDurability);

            return Mathf.Approximately(slot.CurrentDurability, incomingDurability);
        }

        return true;
    }

    private static bool RequiresDedicatedConsumableInstance(ItemData itemData)
    {
        return itemData != null &&
               (!Mathf.Approximately(itemData.RestoreHydration, 0f) ||
                itemData.RestoreCalories != 0);
    }

    private static bool ShouldRemoveSlotAfterConsume(InventorySlot slot)
    {
        if (slot == null || slot.Item == null)
            return true;

        if (slot.HasAmount)
            return slot.CurrentAmount <= ZeroTolerance;

        if (slot.UsesPerInstanceConsumableState)
            return Mathf.Abs(slot.CurrentHydration) <= ZeroTolerance &&
                   Mathf.Abs(slot.CurrentCalories) <= ZeroTolerance;

        return slot.IsEmpty;
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