using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractable
{
    private const string DebugPrefix = "<color=yellow>[WorldItem]</color>";

    [SerializeField] private ItemData _itemData;
    [SerializeField, Min(1)] private int _count = 1;

    public ItemData ItemData => _itemData;
    public int Count => _count;

    [Inject] private InventoryController _inventoryController;

    public void Interact()
    {
        if (_inventoryController == null || _itemData == null)
        {
            return;
        }

        bool success = _inventoryController.TryAddItem(_itemData, _count);

        if (!success)
        {
            Debug.Log($"{DebugPrefix} Could not pick up {_itemData.DisplayName} x{_count}. Inventory full.");

            return;
        }

        Debug.Log($"{DebugPrefix} Picked up {_itemData.DisplayName} x{_count}.");

        Destroy(gameObject);
    }
}