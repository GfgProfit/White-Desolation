using UnityEngine;

public sealed class FireStartCostEntry
{
    public ItemData Item { get; }
    public FireStartCostType Type { get; }
    public int Count { get; }
    public float Amount { get; }
    public float DurabilityCost { get; }

    private FireStartCostEntry(ItemData item, FireStartCostType type, int count, float amount, float durabilityCost)
    {
        Item = item;
        Type = type;
        Count = count;
        Amount = amount;
        DurabilityCost = durabilityCost;
    }

    public static FireStartCostEntry ForItemCount(ItemData item, int count)
    {
        return new FireStartCostEntry(item, FireStartCostType.ItemCount, Mathf.Max(1, count), 0f, 0f);
    }

    public static FireStartCostEntry ForCustomAmount(ItemData item, float amount)
    {
        return new FireStartCostEntry(item, FireStartCostType.CustomAmount, 0, Mathf.Max(0f, amount), 0f);
    }

    public static FireStartCostEntry ForDurability(ItemData item, float durabilityCost)
    {
        return new FireStartCostEntry(item, FireStartCostType.Durability, 0, 0f, Mathf.Max(0f, durabilityCost));
    }
}