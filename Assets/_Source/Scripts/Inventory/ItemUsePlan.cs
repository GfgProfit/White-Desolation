using UnityEngine;

public sealed class ItemUsePlan
{
    private const float ZeroTolerance = 0.0001f;

    public int SlotIndex;
    public ItemPrimaryActionType ActionType;
    public string VerbText;
    public float Duration;

    public float HydrationToApply;
    public float CaloriesToApply;

    public float HydrationStateToConsume;
    public float CaloriesStateToConsume;
    public float AmountToConsume;

    public ItemData ReplaceWhenDepleted;

    public ItemData ReplaceSlotItemAfterAction;
    public bool AutoUseReplacedItem;

    public ItemData ToolItemToDamage;
    public float ToolDurabilityCost;

    public bool HasPlayerEffect => Mathf.Abs(HydrationToApply) > ZeroTolerance || Mathf.Abs(CaloriesToApply) > ZeroTolerance;

    public bool HasInventoryConsume => Mathf.Abs(HydrationStateToConsume) > ZeroTolerance || Mathf.Abs(CaloriesStateToConsume) > ZeroTolerance || Mathf.Abs(AmountToConsume) > ZeroTolerance || ReplaceWhenDepleted != null;

    public bool HasToolDurabilityConsume => ToolItemToDamage != null && ToolDurabilityCost > ZeroTolerance;
}