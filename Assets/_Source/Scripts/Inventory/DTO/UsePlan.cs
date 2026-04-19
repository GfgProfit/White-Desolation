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

    public bool HasInventoryConsume =>
        !Mathf.Approximately(HydrationStateToConsume, 0f) ||
        !Mathf.Approximately(CaloriesStateToConsume, 0f) ||
        !Mathf.Approximately(AmountToConsume, 0f);

    public bool HasPlayerEffect =>
        !Mathf.Approximately(HydrationToApply, 0f) ||
        !Mathf.Approximately(CaloriesToApply, 0f);
}