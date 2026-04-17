using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractable
{
    private const string DebugPrefix = "[WorldItem]";

    [SerializeField] private ItemData _itemData;
    [SerializeField, Min(1)] private int _count = 1;

    [Header("Optional Runtime Overrides")]
    [SerializeField] private bool _overrideCurrentAmount;
    [SerializeField, Min(0.01f)] private float _currentAmount = 1f;

    [SerializeField] private bool _overrideCurrentDurability;
    [SerializeField, Min(0.01f)] private float _currentDurability = 100f;

    [Inject] private InventoryController _inventoryController;

    public ItemData ItemData => _itemData;
    public int Count => _count;

    public void Interact()
    {
        if (_inventoryController == null || _itemData == null)
            return;

        bool success = _inventoryController.TryAddItem(
            _itemData,
            _count,
            _overrideCurrentAmount ? _currentAmount : null,
            _overrideCurrentDurability ? _currentDurability : null);

        if (!success)
        {
            Debug.Log($"{DebugPrefix} Could not pick up {_itemData.DisplayName} x{_count}. Inventory full.");
            return;
        }

        Debug.Log($"{DebugPrefix} Picked up {_itemData.DisplayName} x{_count}.");
        Destroy(gameObject);
    }
}