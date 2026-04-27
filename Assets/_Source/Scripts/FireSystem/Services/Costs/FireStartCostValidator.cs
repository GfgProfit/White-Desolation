using System.Collections.Generic;
using UnityEngine;

public static class FireStartCostValidator
{
    private const float ZeroTolerance = 0.0001f;

    public static bool CanPay(InventoryController inventory, FireStartCost cost)
    {
        if (inventory == null || cost == null)
        {
            return false;
        }

        if (cost.IsEmpty)
        {
            return true;
        }

        List<ItemCountRequirement> itemRequirements = new();
        List<AmountRequirement> amountRequirements = new();

        IReadOnlyList<FireStartCostEntry> entries = cost.Entries;

        for (int i = 0; i < entries.Count; i++)
        {
            FireStartCostEntry entry = entries[i];

            if (entry == null || entry.Item == null)
            {
                return false;
            }

            switch (entry.Type)
            {
                case FireStartCostType.ItemCount:
                    AddItemRequirement(itemRequirements, entry.Item, entry.Count);
                    break;

                case FireStartCostType.CustomAmount:
                    if (!inventory.HasCustomAmount(entry.Item, entry.Amount))
                    {
                        return false;
                    }

                    AddAmountRequirement(amountRequirements, entry.Item, entry.Amount);
                    break;

                case FireStartCostType.Durability:
                    if (!inventory.ContainsUsableItem(entry.Item, 1))
                    {
                        return false;
                    }

                    break;

                default:
                    return false;
            }
        }

        for (int i = 0; i < itemRequirements.Count; i++)
        {
            ItemCountRequirement requirement = itemRequirements[i];

            if (inventory.GetTotalCount(requirement.Item) < requirement.Count)
            {
                return false;
            }
        }

        for (int i = 0; i < amountRequirements.Count; i++)
        {
            AmountRequirement requirement = amountRequirements[i];

            if (inventory.GetTotalAmount(requirement.Item) + ZeroTolerance < requirement.Amount)
            {
                return false;
            }
        }

        return true;
    }

    private static void AddItemRequirement(List<ItemCountRequirement> requirements, ItemData item, int count)
    {
        if (item == null || count <= 0)
        {
            return;
        }

        for (int i = 0; i < requirements.Count; i++)
        {
            if (ItemDataComparer.AreSame(requirements[i].Item, item))
            {
                requirements[i].Count += count;
                return;
            }
        }

        requirements.Add(new ItemCountRequirement(item, count));
    }

    private static void AddAmountRequirement(List<AmountRequirement> requirements, ItemData item, float amount)
    {
        if (item == null || amount <= 0f)
        {
            return;
        }

        for (int i = 0; i < requirements.Count; i++)
        {
            if (ItemDataComparer.AreSame(requirements[i].Item, item))
            {
                requirements[i].Amount += amount;
                return;
            }
        }

        requirements.Add(new AmountRequirement(item, amount));
    }

    private sealed class ItemCountRequirement
    {
        public ItemData Item;
        public int Count;

        public ItemCountRequirement(ItemData item, int count)
        {
            Item = item;
            Count = Mathf.Max(1, count);
        }
    }

    private sealed class AmountRequirement
    {
        public ItemData Item;
        public float Amount;

        public AmountRequirement(ItemData item, float amount)
        {
            Item = item;
            Amount = Mathf.Max(0f, amount);
        }
    }
}