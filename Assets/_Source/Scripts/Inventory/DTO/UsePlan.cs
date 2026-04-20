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

    public ItemData ReplaceSlotItemAfterAction;
    public ItemData ReplaceWhenDepleted;
    public bool AutoUseReplacedItem;

    public ItemData ToolItemToDamage;
    public float ToolDurabilityCost;

    public readonly bool HasToolDurabilityConsume => ToolItemToDamage != null && ToolDurabilityCost > 0.0001f;
    public readonly bool HasInventoryConsume => !Mathf.Approximately(HydrationStateToConsume, 0f) || !Mathf.Approximately(CaloriesStateToConsume, 0f) || !Mathf.Approximately(AmountToConsume, 0f);
    public readonly bool HasPlayerEffect => !Mathf.Approximately(HydrationToApply, 0f) || !Mathf.Approximately(CaloriesToApply, 0f);
}