using System;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    private const float ZeroTolerance = 0.0001f;

    public ItemData Item { get; private set; }
    public int Count { get; private set; } = 1;
    public float CurrentDurability { get; private set; } = 100f;
    public float CurrentAmount { get; private set; } = 0f;
    public float CurrentHydration { get; private set; } = 0f;
    public float CurrentCalories { get; private set; } = 0f;

    public int MaxStack => Item != null ? Item.MaxStack : 1;
    public bool IsEmpty => Item == null || Count <= 0;
    public bool IsFull => !IsEmpty && !HasAmount && !UsesPerInstanceConsumableState && Count >= MaxStack;
    public bool HasDurability => Item != null && Item.UsesDurability && !Item.IsUnbreakable;
    public bool HasAmount => Item != null && Item.UsesCustomAmount;
    public bool HasConsumableState => Item != null && (!Mathf.Approximately(Item.RestoreHydration, 0f) || Item.RestoreCalories != 0);
    public bool UsesPerInstanceConsumableState => HasConsumableState;
    public bool IsBroken => HasDurability && CurrentDurability <= ZeroTolerance;
    public float Durability01 => HasDurability ? Mathf.Clamp01(CurrentDurability / Item.MaxDurability) : 1f;
    public float Amount01 => HasAmount ? Mathf.Clamp01(CurrentAmount / Item.MaxAmount) : 0f;

    public float ConsumableFill01
    {
        get
        {
            if (!HasConsumableState)
            {
                return 1f;
            }

            float hydration01 = -1f;
            float calories01 = -1f;

            if (Item.RestoreHydration > ZeroTolerance)
            {
                hydration01 = Mathf.Clamp01(CurrentHydration / Item.RestoreHydration);
            }

            if (Item.RestoreCalories > 0)
            {
                calories01 = Mathf.Clamp01(CurrentCalories / Item.RestoreCalories);
            }

            if (hydration01 < 0f && calories01 < 0f)
            {
                return 1f;
            }

            return Mathf.Max(0f, hydration01, calories01);
        }
    }

    internal void Initialize(ItemData item, int count, float? currentDurabilityOverride = null, float? currentAmountOverride = null, float? currentHydrationOverride = null, float? currentCaloriesOverride = null)
    {
        Item = item;
        Count = Mathf.Max(1, count);

        if (Item == null)
        {
            CurrentDurability = 100f;
            CurrentAmount = 0f;
            CurrentHydration = 0f;
            CurrentCalories = 0f;
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
                CurrentDurability = Mathf.Clamp(currentDurabilityOverride ?? Item.MaxDurability, 0f, Item.MaxDurability);
            }
        }
        else
        {
            CurrentDurability = 100f;
        }

        if (Item.UsesCustomAmount)
        {
            CurrentAmount = Mathf.Clamp(currentAmountOverride ?? Item.MaxAmount, 0f, Item.MaxAmount);
        }
        else
        {
            CurrentAmount = 0f;
        }

        CurrentHydration = currentHydrationOverride ?? Item.RestoreHydration;
        CurrentCalories = currentCaloriesOverride ?? Item.RestoreCalories;
    }

    public void AddCount(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        Count = Mathf.Min(MaxStack, Count + amount);
    }

    public int RemoveCount(int amount)
    {
        if (amount <= 0)
        {
            return 0;
        }

        int removed = Mathf.Min(Count, amount);
        Count -= removed;

        return removed;
    }

    public void ConsumeDurability(float amount)
    {
        if (amount <= 0f || !HasDurability)
        {
            return;
        }

        CurrentDurability = Mathf.Max(0f, CurrentDurability - amount);
    }

    public void ConsumeHydration(float amount, float zeroTolerance)
    {
        if (Mathf.Approximately(amount, 0f))
        {
            return;
        }

        CurrentHydration -= amount;

        if (Mathf.Abs(CurrentHydration) <= zeroTolerance)
        {
            CurrentHydration = 0f;
        }
    }

    public void ConsumeCalories(float amount, float zeroTolerance)
    {
        if (Mathf.Approximately(amount, 0f))
        {
            return;
        }

        CurrentCalories -= amount;

        if (Mathf.Abs(CurrentCalories) <= zeroTolerance)
        {
            CurrentCalories = 0f;
        }
    }

    public void ConsumeAmount(float amount)
    {
        if (Mathf.Approximately(amount, 0f))
        {
            return;
        }

        CurrentAmount = Mathf.Max(0f, CurrentAmount - amount);
    }

    public float AddAmount(float amount)
    {
        if (amount <= 0f || !HasAmount)
        {
            return 0f;
        }

        float availableAmount = Mathf.Max(0f, Item.MaxAmount - CurrentAmount);
        float addedAmount = Mathf.Min(availableAmount, amount);

        if (addedAmount <= ZeroTolerance)
        {
            return 0f;
        }

        CurrentAmount = Mathf.Min(Item.MaxAmount, CurrentAmount + addedAmount);

        if (Mathf.Abs(Item.MaxAmount - CurrentAmount) <= ZeroTolerance)
        {
            CurrentAmount = Item.MaxAmount;
        }

        return addedAmount;
    }
}
