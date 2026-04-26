using System.Collections.Generic;

public sealed class FireStartCost
{
    private readonly List<FireStartCostEntry> _entries = new();

    public IReadOnlyList<FireStartCostEntry> Entries => _entries;
    public bool IsEmpty => _entries.Count == 0;

    public void AddItem(ItemData item, int count)
    {
        if (item == null || count <= 0)
        {
            return;
        }

        _entries.Add(FireStartCostEntry.ForItemCount(item, count));
    }

    public void AddCustomAmount(ItemData item, float amount)
    {
        if (item == null || amount <= 0f)
        {
            return;
        }

        _entries.Add(FireStartCostEntry.ForCustomAmount(item, amount));
    }

    public void AddDurability(ItemData item, float durabilityCost)
    {
        if (item == null || durabilityCost <= 0f)
        {
            return;
        }

        _entries.Add(FireStartCostEntry.ForDurability(item, durabilityCost));
    }
}