using NaughtyAttributes;
using UnityEngine;

public class WorldItem : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData _itemData;
    [SerializeField, Min(1)] private int _count = 1;

    [Header("Optional Runtime Overrides")]
    [SerializeField] private bool _overrideCurrentAmount;
    [SerializeField, Min(0.01f)] private float _currentAmount = 1f;

    [SerializeField] private bool _overrideCurrentDurability;
    [SerializeField, Min(0.01f)] private float _currentDurability = 100f;

    [Header("Sticking")]
    [SerializeField] private float _stickingOffsetY = 0.1f;

    [Inject] private readonly InventoryController _inventoryController;

    public ItemData ItemData => _itemData;
    public int Count => _count;
    public float CurrentAmount => _overrideCurrentAmount ? _currentAmount : (_itemData != null && _itemData.UsesCustomAmount ? _itemData.MaxAmount : 0f);
    public float CurrentDurability => _overrideCurrentDurability ? _currentDurability : (_itemData != null && _itemData.UsesDurability && !_itemData.IsUnbreakable ? _itemData.MaxDurability : 100f);
    public bool HasDurability => _itemData != null && _itemData.UsesDurability;
    public float CurrentWeightKg => InventoryWeightCalculator.CalculateIncomingWeightKg( _itemData, _count, _overrideCurrentAmount ? _currentAmount : null);

    private void Awake()
    {
        if (Physics.Raycast(transform.localPosition, Vector3.down, out RaycastHit hit, Mathf.Infinity))
        {
            transform.localPosition = hit.point;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - _stickingOffsetY, transform.localPosition.z);
        }
    }

    public void Interact()
    {
        TryPickup();
    }

    public bool TryPickup()
    {
        if (_inventoryController == null || _itemData == null)
        {
            return false;
        }

        bool success = _inventoryController.TryAddItem(_itemData, _count, _overrideCurrentAmount ? _currentAmount : null, _overrideCurrentDurability ? _currentDurability : null);

        if (!success)
        {
            return false;
        }

        Destroy(gameObject);
        return true;
    }

    [Button]
    private void AssignObjectName()
    {
        gameObject.name = $"[Item] - ID: {_itemData.Id}";
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * _stickingOffsetY, 0.1f);
    }
}