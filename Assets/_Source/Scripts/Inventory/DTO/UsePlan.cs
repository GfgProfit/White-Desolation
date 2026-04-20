using UnityEngine;

public struct UsePlan
{
    public int SlotIndex;
    public ItemPrimaryActionType ActionType;
    public string VerbText;
    public float Duration;

    public float HydrationToApply;
    public float CaloriesToApply;

    public float HydrationStateToConsume;
    public float CaloriesStateToConsume;
    public float AmountToConsume;

    public ItemData ReplaceSlotItemAfterAction;   // закрытая банка -> открытая
    public ItemData ReplaceWhenDepleted;          // открытая банка -> пустая
    public bool AutoUseReplacedItem;              // после открытия сразу начать есть

    public ItemData ToolItemToDamage;
    public float ToolDurabilityCost;

    public bool HasToolDurabilityConsume => ToolItemToDamage != null && ToolDurabilityCost > 0.0001f;

    public bool HasInventoryConsume =>
        !Mathf.Approximately(HydrationStateToConsume, 0f) ||
        !Mathf.Approximately(CaloriesStateToConsume, 0f) ||
        !Mathf.Approximately(AmountToConsume, 0f);

    public bool HasPlayerEffect =>
        !Mathf.Approximately(HydrationToApply, 0f) ||
        !Mathf.Approximately(CaloriesToApply, 0f);
}