using UnityEngine;

public sealed class InventoryItemDropper : MonoBehaviour
{
    private const string DebugPrefix = "[InventoryDrop]";

    [Header("Prefab")]
    [SerializeField] private WorldItem _fallbackWorldItemPrefab;

    [Header("Placement")]
    [SerializeField] private Transform _dropOrigin;
    [SerializeField, Min(0f)] private float _dropForwardDistance = 1f;
    [SerializeField] private LayerMask _dropBlockerMask = Physics.DefaultRaycastLayers;

    [Inject] private InventoryController _inventoryController = null;

    public void SetInventoryController(InventoryController inventoryController)
    {
        if (inventoryController != null)
        {
            _inventoryController = inventoryController;
        }
    }

    public bool CanDrop(InventorySlot slot)
    {
        return slot != null && !slot.IsEmpty && slot.Item != null && ResolveWorldPrefab(slot.Item) != null;
    }

    public bool TryDropFromSlot(int slotIndex, int count = 1)
    {
        if (_inventoryController == null)
        {
            Debug.LogWarning($"{DebugPrefix} Cannot drop item without inventory controller.", this);
            return false;
        }

        if (count <= 0)
        {
            return false;
        }

        InventorySlot slot = _inventoryController.GetSlotAt(slotIndex);

        if (!CanDrop(slot))
        {
            return false;
        }

        int dropCount = Mathf.Min(count, slot.Count);
        WorldItem spawnedItem = Spawn(slot, dropCount);

        if (spawnedItem == null)
        {
            return false;
        }

        if (!_inventoryController.TryRemoveFromSlot(slotIndex, dropCount))
        {
            Destroy(spawnedItem.gameObject);
            return false;
        }

        return true;
    }

    private WorldItem Spawn(InventorySlot slot, int count)
    {
        WorldItem prefab = ResolveWorldPrefab(slot.Item);

        if (prefab == null)
        {
            Debug.LogWarning($"{DebugPrefix} No world prefab configured for {slot.Item.DisplayName}.", this);
            return null;
        }

        Transform origin = ResolveDropOrigin();
        Vector3 position = ResolveDropPosition(origin);
        Quaternion rotation = Quaternion.Euler(0f, origin.eulerAngles.y, 0f);

        WorldItem worldItem = Instantiate(prefab, position, rotation);

        float? currentAmountOverride = slot.HasAmount ? slot.CurrentAmount : null;
        float? currentDurabilityOverride = slot.HasDurability ? slot.CurrentDurability : null;

        worldItem.InitializeRuntime(slot.Item, count, currentAmountOverride, currentDurabilityOverride);
        RuntimeObjectInjector.Inject(worldItem.gameObject);

        return worldItem;
    }

    private WorldItem ResolveWorldPrefab(ItemData itemData)
    {
        if (itemData != null && itemData.WorldPrefab != null)
        {
            return itemData.WorldPrefab;
        }

        return _fallbackWorldItemPrefab;
    }

    private Transform ResolveDropOrigin()
    {
        if (_dropOrigin != null)
        {
            return _dropOrigin;
        }

        if (Camera.main != null)
        {
            return Camera.main.transform;
        }

        return transform;
    }

    private Vector3 ResolveDropPosition(Transform origin)
    {
        Vector3 forward = origin.forward.sqrMagnitude > 0.0001f ? origin.forward.normalized : transform.forward.normalized;

        if (Physics.Raycast(origin.position, forward, out RaycastHit hit, _dropForwardDistance, _dropBlockerMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return origin.position + forward * _dropForwardDistance;
    }
}
