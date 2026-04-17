using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    private const string DebugPrefix = "[ItemData]";

    [Header("Base")]
    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField, Multiline(5)] private string _description;
    [SerializeField] private Sprite _icon;

    [Header("Classification")]
    [SerializeField] private ItemCategory _category = ItemCategory.Misc;
    [SerializeField] private ItemPrimaryActionType _primaryAction = ItemPrimaryActionType.Use;

    [Header("Stacking")]
    [SerializeField] private bool _isStackable;
    [SerializeField, ShowIf(nameof(ShowMaxStack)), Min(1)] private int _maxStack = 1;

    [Header("Durability")]
    [SerializeField] private bool _usesDurability;
    [SerializeField, ShowIf(nameof(_usesDurability))] private bool _isUnbreakable;
    [SerializeField, ShowIf(nameof(ShowMaxDurability)), Min(1f)] private float _maxDurability = 100f;

    [Header("Custom Amount")]
    [SerializeField] private bool _usesCustomAmount;
    [SerializeField, ShowIf(nameof(_usesCustomAmount))] private ItemAmountUnit _amountUnit = ItemAmountUnit.Liter;
    [SerializeField, ShowIf(nameof(_usesCustomAmount)), Min(0.01f)] private float _maxAmount = 1f;

    [Header("Consumable Effects")]
    [SerializeField] private float _restoreHydration;
    [SerializeField] private int _restoreCalories;

    public string Id => _id;
    public string DisplayName => _displayName;
    public string Description => _description;
    public Sprite Icon => _icon;

    public ItemCategory Category => _category;
    public ItemPrimaryActionType PrimaryAction => _primaryAction;

    // ВАЖНО:
    // stackable + durability теперь разрешено
    // запрещаем только stackable + custom amount
    public bool IsStackable => !_usesCustomAmount && _isStackable;
    public int MaxStack => IsStackable ? _maxStack : 1;

    public bool UsesDurability => _usesDurability;
    public bool IsUnbreakable => _usesDurability && _isUnbreakable;
    public float MaxDurability => _usesDurability && !_isUnbreakable ? _maxDurability : 100f;

    public bool UsesCustomAmount => _usesCustomAmount;
    public ItemAmountUnit AmountUnit => _amountUnit;
    public float MaxAmount => _usesCustomAmount ? _maxAmount : 0f;

    // Теперь значения могут быть и отрицательными
    public float RestoreHydration => _restoreHydration;
    public int RestoreCalories => _restoreCalories;

    private bool ShowMaxStack() => _isStackable && !_usesCustomAmount;
    private bool ShowMaxDurability() => _usesDurability && !_isUnbreakable;

    private void OnValidate()
    {
        // Только custom amount конфликтует со stack
        if (_usesCustomAmount)
        {
            _isStackable = false;
            _maxStack = 1;
        }

        if (!_isStackable)
            _maxStack = 1;

        if (_maxStack < 1)
            _maxStack = 1;

        if (!_usesDurability)
        {
            _isUnbreakable = false;
            _maxDurability = 100f;
        }
        else if (!_isUnbreakable && _maxDurability < 1f)
        {
            Debug.LogWarning($"{DebugPrefix} MaxDurability for {_displayName} was < 1. Reset to 100.");
            _maxDurability = 100f;
        }

        if (_usesCustomAmount && _maxAmount < 0.01f)
        {
            Debug.LogWarning($"{DebugPrefix} MaxAmount for {_displayName} was too small. Reset to 1.");
            _maxAmount = 1f;
        }

        // отрицательные hydration/calories теперь НЕ режем
    }

    [Button]
    private void BuildId()
    {
        if (string.IsNullOrWhiteSpace(_displayName))
        {
            Debug.LogWarning($"{DebugPrefix} Display name is empty. Cannot build ID.");
            return;
        }

        _id = _displayName.Trim().ToLowerInvariant().Replace(" ", "_");
    }
}