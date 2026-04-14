using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "New Item Data", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    private const string DebugPrefix = "<color=orange>[ItemData]</color>";

    [SerializeField] private string _id;
    [SerializeField] private string _displayName;
    [SerializeField, Multiline(5)] private string _description;
    [SerializeField] private bool _isStackable;
    [SerializeField, ShowIf(nameof(_isStackable)), Min(1)] private int _maxStack = 1;
    [SerializeField] private Sprite _icon;

    public string Id => _id;
    public string DisplayName => _displayName;
    public string Description => _description;
    public bool IsStackable => _isStackable;
    public int MaxStack => _isStackable ? _maxStack : 1;
    public Sprite Icon => _icon;

    private void OnValidate()
    {
        if (!_isStackable)
        {
            _maxStack = 1;
        }
        else if (_maxStack < 1)
        {
            Debug.LogWarning($"{DebugPrefix} MaxStack for {_displayName} was < 1. Reset to 1.");
            _maxStack = 1;
        }
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