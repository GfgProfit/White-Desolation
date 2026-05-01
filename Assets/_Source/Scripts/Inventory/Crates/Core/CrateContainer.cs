using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SaveId))]
public sealed partial class CrateContainer : MonoBehaviour, IInteractable, IInteractHoverInfo, IInteractionExtraInfoProvider, ISaveable
{
    private const string DebugPrefix = "[Crate]";

    [Header("Interaction")]
    [SerializeField] private string _interactionText = "Container";
    [SerializeField, Min(0.01f)] private float _searchDurationSeconds = 5f;
    [SerializeField] private CrateUIController _uiController;

    [Header("Weight")]
    [SerializeField, Min(0f)] private float _maxWeightKg = 15f;

    [Header("Loot")]
    [SerializeField, Min(0)] private int _maxGeneratedItemCount = 4;
    [SerializeField] private CrateLootEntry[] _lootTable;

    [Header("Save")]
    [SerializeField] private SaveId _saveId;
    [SerializeField] private bool _lootGenerated;
    [SerializeField] private bool _searched;

    private readonly List<InventorySlot> _items = new();

    public string SaveId => _saveId != null ? _saveId.Id : string.Empty;
    public IReadOnlyList<InventorySlot> Items => _items;
    public float MaxWeightKg => _maxWeightKg;
    public float CurrentWeightKg => InventoryWeightCalculator.CalculateTotalWeightKg(_items);
    public float SearchDurationSeconds => _searchDurationSeconds;
    public bool IsSearched => _searched;
    public bool HasItems => _items.Count > 0;

    public event Action OnChanged;

    private void Reset()
    {
        CacheSaveId();
    }

    private void Awake()
    {
        CacheSaveId();
        EnsureLootGenerated();
    }

    private void CacheSaveId()
    {
        if (_saveId == null)
        {
            _saveId = GetComponent<SaveId>();
        }
    }

    private bool FinishMutation(bool mutated)
    {
        if (mutated)
        {
            NotifyChanged();
        }

        return mutated;
    }

    private void NotifyChanged()
    {
        OnChanged?.Invoke();
    }
}
