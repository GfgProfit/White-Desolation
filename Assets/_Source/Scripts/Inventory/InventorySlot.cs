using System;
using UnityEngine;

[Serializable]
public class InventorySlot
{
    private const float ZeroTolerance = 0.0001f;

    public ItemData Item;
    public int Count = 1;
    public float CurrentDurability = 100f;
    public float CurrentAmount = 0f;

    // Остатки consumable-эффектов
    public float CurrentHydration = 0f;
    public float CurrentCalories = 0f;

    public int MaxStack => Item != null ? Item.MaxStack : 1;
    public bool IsEmpty => Item == null || Count <= 0;
    public bool IsFull => !IsEmpty && !HasAmount && !UsesPerInstanceConsumableState && Count >= MaxStack;

    public bool HasDurability => Item != null && Item.UsesDurability && !Item.IsUnbreakable;
    public bool HasAmount => Item != null && Item.UsesCustomAmount;

    public bool HasConsumableState => Item != null &&
                                      (!Mathf.Approximately(Item.RestoreHydration, 0f) ||
                                       Item.RestoreCalories != 0);

    public bool UsesPerInstanceConsumableState => HasConsumableState;

    public float Durability01 => HasDurability
        ? Mathf.Clamp01(CurrentDurability / Item.MaxDurability)
        : 1f;

    public float Amount01 => HasAmount
        ? Mathf.Clamp01(CurrentAmount / Item.MaxAmount)
        : 0f;

    /// <summary>
    /// Нормализованный остаток consumable-содержимого.
    /// Для предметов с двумя эффектами (например, сода) берём максимум из оставшихся долей,
    /// чтобы вес не падал быстрее, чем реально опустошается предмет.
    /// При корректном пропорциональном использовании оба значения обычно одинаковые.
    /// </summary>
    public float ConsumableFill01
    {
        get
        {
            if (!HasConsumableState)
                return 1f;

            float hydration01 = -1f;
            float calories01 = -1f;

            if (Item.RestoreHydration > ZeroTolerance)
                hydration01 = Mathf.Clamp01(CurrentHydration / Item.RestoreHydration);

            if (Item.RestoreCalories > 0)
                calories01 = Mathf.Clamp01(CurrentCalories / Item.RestoreCalories);

            if (hydration01 < 0f && calories01 < 0f)
                return 1f;

            return Mathf.Max(0f, hydration01, calories01);
        }
    }

    public void Initialize(
        ItemData item,
        int count,
        float? currentDurabilityOverride = null,
        float? currentAmountOverride = null,
        float? currentHydrationOverride = null,
        float? currentCaloriesOverride = null)
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

        CurrentHydration = currentHydrationOverride ?? Item.RestoreHydration;
        CurrentCalories = currentCaloriesOverride ?? Item.RestoreCalories;
    }
}