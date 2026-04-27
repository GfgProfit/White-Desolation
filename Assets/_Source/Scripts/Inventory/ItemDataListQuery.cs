using System.Collections.Generic;

public static class ItemDataListQuery
{
    public static bool Contains(IReadOnlyList<ItemData> items, ItemData item)
    {
        if (items == null || item == null)
        {
            return false;
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (ItemDataComparer.AreSame(items[i], item))
            {
                return true;
            }
        }

        return false;
    }
}