using UnityEngine;

public partial class WorldItem
{
    public void Interact()
    {
        TryPickup();
    }

    public bool TryPickup()
    {
        if (_pickedUp)
        {
            return false;
        }

        if (_inventoryController == null || _itemData == null)
        {
            return false;
        }

        bool success = _inventoryController.TryAddItem(_itemData, _count, _overrideCurrentAmount ? _currentAmount : null, _overrideCurrentDurability ? _currentDurability : null);

        if (!success)
        {
            Debug.Log($"{DebugPrefix} Could not pick up {_itemData.DisplayName} x{_count}. Inventory full.");
            return false;
        }

        Debug.Log($"{DebugPrefix} Picked up {_itemData.DisplayName} x{_count}.");

        SetPickedUp(true);

        return true;
    }

    private void SetPickedUp(bool pickedUp)
    {
        _pickedUp = pickedUp;
        gameObject.SetActive(!pickedUp);
    }
}
