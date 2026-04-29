using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(SaveId))]
public partial class WorldItem : MonoBehaviour, IInteractable, IInteractHoverInfo, IInspectableInteractable, ISaveable
{
    private const string DebugPrefix = "[WorldItem]";

    [SerializeField] private ItemData _itemData;
    [SerializeField, Min(1)] private int _count = 1;
    [SerializeField] private float _stickingOffsetY = -0.5f;

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
    public ItemData ItemData => _itemData;
    public int Count => _count;
    public bool PickedUp => _pickedUp;
    public float CurrentAmount => _overrideCurrentAmount ? _currentAmount : (_itemData != null && _itemData.UsesCustomAmount ? _itemData.MaxAmount : 0f);
    public float CurrentDurability => _overrideCurrentDurability ? _currentDurability : (_itemData != null && _itemData.UsesDurability && !_itemData.IsUnbreakable ? _itemData.MaxDurability : 100f);
    public bool HasDurability => _itemData != null && _itemData.UsesDurability;
    public float CurrentWeightKg => InventoryWeightCalculator.CalculateIncomingWeightKg(_itemData, _count, _overrideCurrentAmount ? _currentAmount : null);
    public bool CanInspect => !_pickedUp && _itemData != null;
}
