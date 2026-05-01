using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[CreateAssetMenu(fileName = "New Craft Recipe", menuName = "Inventory/Craft Recipe")]
public sealed class CraftRecipe : ScriptableObject
{
    [Header("Base")]
    [SerializeField] private string _id;
    [SerializeField] private ItemData _resultItem;
    [SerializeField] private Sprite _craftListIcon;
    [SerializeField, Min(1)] private int _resultCount = 1;

    [Header("Requirements")]
    [SerializeField] private CraftRequirement[] _requirements;
    [SerializeField] private CraftToolRequirement[] _toolRequirements;

    [Header("Time")]
    [SerializeField, Min(0f)] private float _gameMinutes = 60f;

    public string Id => !string.IsNullOrWhiteSpace(_id) ? _id : name;
    public ItemData ResultItem => _resultItem;
    public Sprite CraftListIcon => _craftListIcon != null ? _craftListIcon : _resultItem != null ? _resultItem.Icon : null;
    public int ResultCount => Mathf.Max(1, _resultCount);
    public IReadOnlyList<CraftRequirement> Requirements => _requirements;
    public IReadOnlyList<CraftToolRequirement> ToolRequirements => _toolRequirements;
    public float GameMinutes => Mathf.Max(0f, _gameMinutes);
    public bool IsValid => _resultItem != null && _resultCount > 0;

    private void OnValidate()
    {
        if (_resultCount < 1)
        {
            _resultCount = 1;
        }

        if (_gameMinutes < 0f)
        {
            _gameMinutes = 0f;
        }
    }

    [Button]
    private void BuildId()
    {
        if (_resultItem == null)
        {
            return;
        }

        _id = $"{_resultItem.Id}_recipe";
    }
}
