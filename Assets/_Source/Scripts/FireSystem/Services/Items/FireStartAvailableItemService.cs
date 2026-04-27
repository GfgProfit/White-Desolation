using System.Collections.Generic;

public sealed class FireStartAvailableItemService
{
    private readonly InventoryController _inventory;
    private readonly float _accelerantAmountCost;

    public FireStartAvailableItemService(InventoryController inventory, float accelerantAmountCost)
    {
        _inventory = inventory;
        _accelerantAmountCost = accelerantAmountCost;
    }

    public void Rebuild(FireStartingConfig config, List<ItemData> igniters, List<ItemData> tinders, List<ItemData> fuels, List<ItemData> accelerants)
    {
        if (igniters != null)
        {
            FillAvailable(igniters, config != null ? config.Igniters : null, false);
        }

        if (tinders != null)
        {
            FillAvailable(tinders, config != null ? config.Tinders : null, false);
        }

        if (fuels != null)
        {
            FillAvailable(fuels, config != null ? config.Fuels : null, false);
        }

        if (accelerants != null)
        {
            FillAvailable(accelerants, config != null ? config.Accelerants : null, true);
        }
    }

    private void FillAvailable(List<ItemData> result, ItemData[] source, bool isAccelerant)
    {
        result.Clear();

        if (source == null)
        {
            return;
        }

        for (int i = 0; i < source.Length; i++)
        {
            ItemData item = source[i];

            if (item == null)
            {
                continue;
            }

            if (ContainsSameItem(result, item))
            {
                continue;
            }

            bool available;

            if (isAccelerant && item.UsesCustomAmount)
            {
                available = HasCustomAmount(item, _accelerantAmountCost);
            }
            else
            {
                available = _inventory != null && _inventory.ContainsUsableItem(item, 1);
            }

            if (available)
            {
                result.Add(item);
            }
        }
    }

    private bool HasCustomAmount(ItemData item, float requiredAmount)
    {
        if (_inventory == null || item == null)
        {
            return false;
        }

        for (int i = 0; i < _inventory.Items.Count; i++)
        {
            InventorySlot slot = _inventory.Items[i];

            if (slot == null || slot.IsEmpty || slot.Item == null)
            {
                continue;
            }

            if (!ItemDataComparer.AreSame(slot.Item, item))
            {
                continue;
            }

            if (!slot.HasAmount)
            {
                continue;
            }

            if (slot.CurrentAmount >= requiredAmount)
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsSameItem(List<ItemData> items, ItemData item)
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