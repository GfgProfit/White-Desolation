using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SaveId))]
public sealed class CrateContainer : MonoBehaviour, IInteractable, IInteractHoverInfo, IInteractionExtraInfoProvider, ISaveable
{
    private const float ZeroTolerance = 0.0001f;

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

    public event System.Action OnChanged;

    private void Reset()
    {
        _saveId = GetComponent<SaveId>();
    }

    private void Awake()
    {
        if (_saveId == null)
        {
            _saveId = GetComponent<SaveId>();
        }

        EnsureLootGenerated();
    }

    public InteractionHoverInfo GetHoverInfo()
    {
        return new InteractionHoverInfo
        {
            InteractionText = _interactionText
        };
    }

    public bool TryGetExtraInfo(out string infoText)
    {
        infoText = string.Empty;

        if (!_searched)
        {
            return false;
        }

        infoText = HasItems ? "Обыскано" : "Пусто";
        return true;
    }

    public void Interact()
    {
        EnsureLootGenerated();

        CrateUIController uiController = ResolveUIController();

        if (uiController == null)
        {
            Debug.LogWarning($"[Crate] Cannot interact with '{name}' without CrateUIController.", this);
            return;
        }

        if (!_searched)
        {
            uiController.BeginSearch(this);
            return;
        }

        uiController.OpenCrate(this);
    }

    public void MarkSearched()
    {
        if (_searched)
        {
            return;
        }

        _searched = true;
        NotifyChanged();
    }

    public bool TryAddFromSlot(InventorySlot sourceSlot, int count = 1)
    {
        if (sourceSlot == null || sourceSlot.IsEmpty || sourceSlot.Item == null)
        {
            return false;
        }

        int itemCount = Mathf.Clamp(count, 1, sourceSlot.Count);
        float? currentAmount = sourceSlot.HasAmount ? sourceSlot.CurrentAmount : null;
        float? currentDurability = sourceSlot.HasDurability ? sourceSlot.CurrentDurability : null;
        float? currentHydration = sourceSlot.HasConsumableState ? sourceSlot.CurrentHydration : null;
        float? currentCalories = sourceSlot.HasConsumableState ? sourceSlot.CurrentCalories : null;

        return TryAddItem(sourceSlot.Item, itemCount, currentAmount, currentDurability, currentHydration, currentCalories);
    }

    public bool TryAddItem(ItemData item, int count, float? currentAmountOverride = null, float? currentDurabilityOverride = null, float? currentHydrationOverride = null, float? currentCaloriesOverride = null)
    {
        if (!InventoryAddCapacityPolicy.CanAddItem(item, count, currentAmountOverride))
        {
            return false;
        }

        float incomingWeight = InventoryWeightCalculator.CalculateIncomingWeightKg(item, count, currentAmountOverride, currentHydrationOverride, currentCaloriesOverride);

        if (!InventoryCapacityPolicy.CanAcceptWeight(CurrentWeightKg, _maxWeightKg, incomingWeight))
        {
            return false;
        }

        bool added = AddToSlots(item, count, currentAmountOverride, currentDurabilityOverride, currentHydrationOverride, currentCaloriesOverride);

        if (added)
        {
            NotifyChanged();
        }

        return added;
    }

    public bool TryRemoveFromSlot(int slotIndex, int count = 1)
    {
        bool removed = InventorySlotRemovalService.TryRemoveFromSlot(_items, slotIndex, count);

        if (removed)
        {
            NotifyChanged();
        }

        return removed;
    }

    public bool TryRemoveFromSlot(InventorySlot slot, int count = 1)
    {
        int slotIndex = IndexOf(slot);

        if (slotIndex < 0)
        {
            return false;
        }

        return TryRemoveFromSlot(slotIndex, count);
    }

    public int IndexOf(InventorySlot slot)
    {
        if (slot == null)
        {
            return -1;
        }

        for (int i = 0; i < _items.Count; i++)
        {
            if (ReferenceEquals(_items[i], slot))
            {
                return i;
            }
        }

        return -1;
    }

    public void CaptureState(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        if (saveData.Crates == null)
        {
            saveData.Crates = new List<CrateSaveData>();
        }

        RemoveSaveData(saveData.Crates, SaveId);

        CrateSaveData crateSaveData = new()
        {
            SaveId = SaveId,
            LootGenerated = _lootGenerated,
            Searched = _searched
        };

        for (int i = 0; i < _items.Count; i++)
        {
            if (InventorySlotSaveDataMapper.TryCreateSaveData(_items[i], out InventorySlotSaveData slotSaveData))
            {
                crateSaveData.Items.Add(slotSaveData);
            }
        }

        saveData.Crates.Add(crateSaveData);
    }

    public void RestoreState(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null || saveData.Crates == null)
        {
            return;
        }

        CrateSaveData crateSaveData = FindSaveData(saveData.Crates, SaveId);

        if (crateSaveData == null)
        {
            return;
        }

        _lootGenerated = crateSaveData.LootGenerated;
        _searched = crateSaveData.Searched;
        _items.Clear();

        if (crateSaveData.Items != null)
        {
            for (int i = 0; i < crateSaveData.Items.Count; i++)
            {
                if (InventorySlotSaveDataMapper.TryCreateSlot(crateSaveData.Items[i], context, out InventorySlot slot))
                {
                    _items.Add(slot);
                }
            }
        }

        NotifyChanged();
    }

    private void EnsureLootGenerated()
    {
        if (_lootGenerated)
        {
            return;
        }

        _lootGenerated = true;

        if (_lootTable == null || _lootTable.Length == 0 || _maxGeneratedItemCount <= 0)
        {
            return;
        }

        int targetCount = Random.Range(0, _maxGeneratedItemCount + 1);
        int spawnedCount = 0;
        List<CrateLootEntry> candidates = new(_lootTable);
        Shuffle(candidates);

        for (int i = 0; i < candidates.Count && spawnedCount < targetCount; i++)
        {
            CrateLootEntry entry = candidates[i];

            if (entry == null || entry.Item == null)
            {
                continue;
            }

            if (Random.value > entry.Chance)
            {
                continue;
            }

            int remainingCount = targetCount - spawnedCount;
            int count = Mathf.Min(remainingCount, Random.Range(entry.MinCount, entry.MaxCount + 1));

            if (TryAddItem(entry.Item, count))
            {
                spawnedCount += count;
            }
        }
    }

    private bool AddToSlots(ItemData item, int count, float? currentAmountOverride, float? currentDurabilityOverride, float? currentHydrationOverride, float? currentCaloriesOverride)
    {
        if (item == null || count <= 0)
        {
            return false;
        }

        if (item.UsesCustomAmount)
        {
            return InventoryCustomAmountAddService.TryAddCustomAmountItem(_items, item, count, currentAmountOverride, currentDurabilityOverride, ZeroTolerance);
        }

        if (InventoryConsumableInstancePolicy.RequiresDedicatedInstance(item))
        {
            return AddSeparateSlots(item, count, currentDurabilityOverride, currentAmountOverride, currentHydrationOverride, currentCaloriesOverride);
        }

        if (item.IsStackable)
        {
            return InventoryStackableAddService.TryAddStackableItems(_items, item, count, currentDurabilityOverride, currentAmountOverride);
        }

        return AddSeparateSlots(item, count, currentDurabilityOverride, currentAmountOverride, currentHydrationOverride, currentCaloriesOverride);
    }

    private bool AddSeparateSlots(ItemData item, int count, float? currentDurabilityOverride, float? currentAmountOverride, float? currentHydrationOverride, float? currentCaloriesOverride)
    {
        for (int i = 0; i < count; i++)
        {
            _items.Add(InventorySlotFactory.Create(item, 1, currentDurabilityOverride, currentAmountOverride, currentHydrationOverride, currentCaloriesOverride));
        }

        return true;
    }

    private CrateUIController ResolveUIController()
    {
        if (_uiController != null)
        {
            return _uiController;
        }

        _uiController = FindFirstObjectByType<CrateUIController>(FindObjectsInactive.Include);

        if (_uiController == null)
        {
            GameObject controllerObject = new("Crate UI Controller");
            _uiController = controllerObject.AddComponent<CrateUIController>();
            SceneInstaller.Container?.InjectGameObject(controllerObject, true);
        }

        return _uiController;
    }

    private void NotifyChanged()
    {
        OnChanged?.Invoke();
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }

    private static CrateSaveData FindSaveData(List<CrateSaveData> crates, string saveId)
    {
        if (crates == null || string.IsNullOrWhiteSpace(saveId))
        {
            return null;
        }

        for (int i = 0; i < crates.Count; i++)
        {
            CrateSaveData crate = crates[i];

            if (crate != null && crate.SaveId == saveId)
            {
                return crate;
            }
        }

        return null;
    }

    private static void RemoveSaveData(List<CrateSaveData> crates, string saveId)
    {
        if (crates == null || string.IsNullOrWhiteSpace(saveId))
        {
            return;
        }

        for (int i = crates.Count - 1; i >= 0; i--)
        {
            if (crates[i] != null && crates[i].SaveId == saveId)
            {
                crates.RemoveAt(i);
            }
        }
    }
}
