using System;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    public ItemData Item;
    public int Count = 1;
    public float CurrentDurability = 100f;
    public float CurrentAmount = 0f;

    public int MaxStack => Item != null ? Item.MaxStack : 1;
    public bool IsEmpty => Item == null || Count <= 0;
    public bool IsFull => !IsEmpty && !HasAmount && Count >= MaxStack;

    public bool HasDurability => Item != null && Item.UsesDurability && !Item.IsUnbreakable;
    public bool HasAmount => Item != null && Item.UsesCustomAmount;

    public float Durability01 => HasDurability
        ? Mathf.Clamp01(CurrentDurability / Item.MaxDurability)
        : 1f;

    public float Amount01 => HasAmount
        ? Mathf.Clamp01(CurrentAmount / Item.MaxAmount)
        : 0f;

    public void Initialize(ItemData item, int count, float? currentDurabilityOverride = null, float? currentAmountOverride = null)
    {
        Item = item;
        Count = Mathf.Max(1, count);

        if (Item == null)
        {
            CurrentDurability = 100f;
            CurrentAmount = 0f;
            return;
        }

        if (Item.UsesDurability)
        {
            if (Item.IsUnbreakable)
            {
                CurrentDurability = 100f;
            }
            else
            {
                CurrentDurability = Mathf.Clamp(
                    currentDurabilityOverride ?? Item.MaxDurability,
                    0f,
                    Item.MaxDurability);
            }
        }
        else
        {
            CurrentDurability = 100f;
        }

        if (Item.UsesCustomAmount)
        {
            CurrentAmount = Mathf.Clamp(
                currentAmountOverride ?? Item.MaxAmount,
                0f,
                Item.MaxAmount);
        }
        else
        {
            CurrentAmount = 0f;
        }
    }
}