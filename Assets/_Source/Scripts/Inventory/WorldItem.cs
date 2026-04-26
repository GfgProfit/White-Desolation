using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SaveId))]
public class WorldItem : MonoBehaviour, IInteractable, ISaveable
{
    private const string DebugPrefix = "[WorldItem]";

    [SerializeField] private ItemData _itemData;
    [SerializeField, Min(1)] private int _count = 1;
    [SerializeField] private float _stickingOffsetY = -0.5f;

    [Header("Optional Runtime Overrides")]
    [SerializeField] private bool _overrideCurrentAmount;
    [SerializeField, Min(0.01f)] private float _currentAmount = 1f;

    [SerializeField] private bool _overrideCurrentDurability;
    [SerializeField, Min(0.01f)] private float _currentDurability = 100f;

    [Header("Save")]
    [SerializeField] private SaveId _saveId;
    [SerializeField] private bool _pickedUp;

    [Inject] private InventoryController _inventoryController;

    public string SaveId => _saveId != null ? _saveId.Id : string.Empty;

    public ItemData ItemData => _itemData;
    public int Count => _count;

    public bool PickedUp => _pickedUp;

    public float CurrentAmount =>
        _overrideCurrentAmount
            ? _currentAmount
            : (_itemData != null && _itemData.UsesCustomAmount ? _itemData.MaxAmount : 0f);

    public float CurrentDurability =>
        _overrideCurrentDurability
            ? _currentDurability
            : (_itemData != null && _itemData.UsesDurability && !_itemData.IsUnbreakable ? _itemData.MaxDurability : 100f);

    public bool HasDurability => _itemData != null && _itemData.UsesDurability;

    public float CurrentWeightKg => InventoryWeightCalculator.CalculateIncomingWeightKg(
        _itemData,
        _count,
        _overrideCurrentAmount ? _currentAmount : null);

    private void Reset()
    {
        _saveId = GetComponent<SaveId>();
    }

    private void Awake()
    {
        if (Physics.Raycast(transform.localPosition, Vector3.down, out RaycastHit hit, Mathf.Infinity))
        {
            transform.localPosition = hit.point;
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - _stickingOffsetY, transform.localPosition.z);
        }

        if (_saveId == null)
        {
            _saveId = GetComponent<SaveId>();
        }

        if (_pickedUp)
        {
            gameObject.SetActive(false);
        }
    }

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

        bool success = _inventoryController.TryAddItem(
            _itemData,
            _count,
            _overrideCurrentAmount ? _currentAmount : null,
            _overrideCurrentDurability ? _currentDurability : null);

        if (!success)
        {
            Debug.Log($"{DebugPrefix} Could not pick up {_itemData.DisplayName} x{_count}. Inventory full.");
            return false;
        }

        Debug.Log($"{DebugPrefix} Picked up {_itemData.DisplayName} x{_count}.");

        SetPickedUp(true);

        return true;
    }

    public void CaptureState(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(SaveId))
        {
            Debug.LogWarning($"{DebugPrefix} Cannot save WorldItem without SaveId: {name}");
            return;
        }

        RemoveOldState(saveData.WorldItems, SaveId);

        saveData.WorldItems.Add(new WorldItemSaveData
        {
            SaveId = SaveId,
            PickedUp = _pickedUp,
            ItemId = _itemData != null ? _itemData.Id : string.Empty,
            Count = _count,
            OverrideCurrentAmount = _overrideCurrentAmount,
            CurrentAmount = _currentAmount,
            OverrideCurrentDurability = _overrideCurrentDurability,
            CurrentDurability = _currentDurability,
            Position = new SerializableVector3(transform.position),
            Rotation = new SerializableQuaternion(transform.rotation)
        });
    }

    public void RestoreState(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null || saveData.WorldItems == null)
        {
            return;
        }

        WorldItemSaveData itemSaveData = FindState(saveData.WorldItems, SaveId);

        if (itemSaveData == null)
        {
            return;
        }

        if (context != null &&
            context.ItemDatabase != null &&
            !string.IsNullOrWhiteSpace(itemSaveData.ItemId) &&
            context.ItemDatabase.TryGetItem(itemSaveData.ItemId, out ItemData restoredItem))
        {
            _itemData = restoredItem;
        }

        _count = Mathf.Max(1, itemSaveData.Count);

        _overrideCurrentAmount = itemSaveData.OverrideCurrentAmount;
        _currentAmount = itemSaveData.CurrentAmount;

        _overrideCurrentDurability = itemSaveData.OverrideCurrentDurability;
        _currentDurability = itemSaveData.CurrentDurability;

        transform.SetPositionAndRotation(
            itemSaveData.Position.ToVector3(),
            itemSaveData.Rotation.ToQuaternion());

        SetPickedUp(itemSaveData.PickedUp);
    }

    private void SetPickedUp(bool pickedUp)
    {
        _pickedUp = pickedUp;
        gameObject.SetActive(!pickedUp);
    }

    private static WorldItemSaveData FindState(List<WorldItemSaveData> states, string saveId)
    {
        if (states == null || string.IsNullOrWhiteSpace(saveId))
        {
            return null;
        }

        for (int i = 0; i < states.Count; i++)
        {
            WorldItemSaveData state = states[i];

            if (state != null && state.SaveId == saveId)
            {
                return state;
            }
        }

        return null;
    }

    private static void RemoveOldState(List<WorldItemSaveData> states, string saveId)
    {
        if (states == null || string.IsNullOrWhiteSpace(saveId))
        {
            return;
        }

        for (int i = states.Count - 1; i >= 0; i--)
        {
            if (states[i] != null && states[i].SaveId == saveId)
            {
                states.RemoveAt(i);
            }
        }
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