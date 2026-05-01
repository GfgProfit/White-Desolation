using UnityEngine;

[RequireComponent(typeof(SaveId))]
public partial class WorldItem : MonoBehaviour, IInteractable, IInteractHoverInfo, IInspectableInteractable, ISaveable
{
    private const string DebugPrefix = "[WorldItem]";

    [SerializeField] private ItemData _itemData;
    [SerializeField, Min(1)] private int _count = 1;

    [Header("Optional Runtime Overrides")]
    [SerializeField] private bool _overrideCurrentAmount;
    [SerializeField] private float _currentAmount = 1f;

    [SerializeField] private bool _overrideCurrentDurability;
    [SerializeField] private float _currentDurability = 100f;

    [Header("Save")]
    [SerializeField] private SaveId _saveId;
    [SerializeField] private bool _pickedUp;

    [Inject] private InventoryController _inventoryController = null;

    public string SaveId => _saveId != null ? _saveId.Id : string.Empty;
    public bool IsRuntimeSpawned { get; private set; }
    public ItemData ItemData => _itemData;
    public int Count => _count;
    public bool PickedUp => _pickedUp;
    public float CurrentAmount => _overrideCurrentAmount ? _currentAmount : (_itemData != null && _itemData.UsesCustomAmount ? _itemData.MaxAmount : 0f);
    public float CurrentDurability => _overrideCurrentDurability ? _currentDurability : (_itemData != null && _itemData.UsesDurability && !_itemData.IsUnbreakable ? _itemData.MaxDurability : 100f);
    public bool HasDurability => _itemData != null && _itemData.UsesDurability;
    public float CurrentWeightKg => InventoryWeightCalculator.CalculateIncomingWeightKg(_itemData, _count, _overrideCurrentAmount ? _currentAmount : null);
    public bool CanInspect => !_pickedUp && _itemData != null;

    public void InitializeRuntime(ItemData itemData, int count, float? currentAmountOverride = null, float? currentDurabilityOverride = null, bool regenerateSaveId = true, string saveId = null)
    {
        IsRuntimeSpawned = true;

        _itemData = itemData;
        _count = Mathf.Max(1, count);

        _overrideCurrentAmount = itemData != null && itemData.UsesCustomAmount && currentAmountOverride.HasValue;
        _currentAmount = currentAmountOverride ?? (itemData != null && itemData.UsesCustomAmount ? itemData.MaxAmount : 0f);

        _overrideCurrentDurability = itemData != null && itemData.UsesDurability && !itemData.IsUnbreakable && currentDurabilityOverride.HasValue;
        _currentDurability = currentDurabilityOverride ?? (itemData != null && itemData.UsesDurability && !itemData.IsUnbreakable ? itemData.MaxDurability : 100f);

        if (_saveId == null)
        {
            _saveId = GetComponent<SaveId>();
        }

        if (_saveId == null)
        {
            _saveId = gameObject.AddComponent<SaveId>();
        }

        if (!string.IsNullOrWhiteSpace(saveId))
        {
            _saveId.AssignId(saveId);
        }
        else if (regenerateSaveId)
        {
            _saveId.AssignNewId();
        }

        SetPickedUp(false);
    }
}
