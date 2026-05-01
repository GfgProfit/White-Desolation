using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public partial class InventoryUIController : MonoBehaviour
{
    private const float ZeroTolerance = 0.0001f;
    private const float SelectedCategoryIconAlpha = 1f;
    private const float UnselectedCategoryIconAlpha = 0.2f;

    private static readonly Color SelectedCategoryButtonColor = new Color32(0x30, 0x3B, 0x37, 0xFF); // #303B37
    private static readonly Color UnselectedCategoryButtonColor = new Color32(0x19, 0x1D, 0x1E, 0xFF); // #191D1E

    [Header("Root")]
    [SerializeField] private GameObject _inventoryRoot;

    [Header("Grid")]
    [SerializeField] private Transform _gridRoot;
    [SerializeField] private InventoryItemCellView _cellPrefab;

    [Header("Category Filters")]
    [SerializeField] private CategoryFilterButton[] _categoryFilterButtons;

    [Header("Sort")]
    [SerializeField] private SortButtonConfig[] _sortButtons;
    [SerializeField] private InventorySortMode _defaultSortMode = InventorySortMode.Name;
    [SerializeField] private InventorySortDirection _defaultSortDirection = InventorySortDirection.Ascending;
    [SerializeField, Range(0f, 1f)] private float _selectedSortButtonAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float _unselectedSortButtonAlpha = 0.2f;

    [Header("Weight Display")]
    [SerializeField] private TMP_Text _carryWeightText;
    [SerializeField] private TMP_Text _currentWeightText;
    [SerializeField] private Slider _carryWeightSlider;

    [Header("Details")]
    [SerializeField] private Image _itemIcon;
    [SerializeField] private TMP_Text _itemNameText;
    [SerializeField] private TMP_Text _itemDescriptionText;
    [SerializeField] private TMP_Text _itemCountText;

    [Header("Stats")]
    [SerializeField] private GameObject _durabilityHolder;
    [SerializeField] private TMP_Text _durabilityText;
    [SerializeField] private Image _durabilityIcon;
    [SerializeField] private GameObject _weightHolder;
    [SerializeField] private TMP_Text _weightText;
    [SerializeField] private GameObject _caloriesHolder;
    [SerializeField] private TMP_Text _caloriesText;
    [SerializeField] private GameObject _hydrationHolder;
    [SerializeField] private TMP_Text _hydrationText;

    [Header("Buttons")]
    [SerializeField] private Button _useButton;
    [SerializeField] private TMP_Text _useButtonLabel;
    [SerializeField] private Button _dropOneButton;

    [Header("Drop")]
    [SerializeField] private InventoryItemDropper _itemDropper;

    [Header("Use Progress Modal")]
    [SerializeField] private GameObject _useProgressModalRoot;
    [SerializeField] private Image _useProgressFillImage;
    [SerializeField] private TMP_Text _useProgressText;
    [SerializeField, Min(0.01f)] private float _useDurationSeconds = 5f;

    [Header("Scene References")]
    [FormerlySerializedAs("_playerNeedsController")]
    [SerializeField] private MonoBehaviour _playerNeedsSource;

    [Header("Optional")]
    [SerializeField] private Behaviour[] _disableWhileOpen;
    [FormerlySerializedAs("_obectDisableWhileOpen")]
    [SerializeField] private GameObject[] _objectDisableWhileOpen;

    [Inject] private InventoryController _inventoryController = null;
    [Inject] private IPlayerInput _playerInput = null;

    public GameObject InventoryRoot => _inventoryRoot;
    public Transform GridRoot => _gridRoot;
    public InventoryItemCellView CellPrefab => _cellPrefab;
    public GameObject UseProgressModalRoot => _useProgressModalRoot;
    public Image UseProgressFillImage => _useProgressFillImage;
    public TMP_Text UseProgressText => _useProgressText;
    public Behaviour[] DisableWhileOpen => _disableWhileOpen;
    public GameObject[] ObjectDisableWhileOpen => _objectDisableWhileOpen;
    public InventoryCategoryFilter ActiveFilter => _activeFilter;
    public InventorySortMode ActiveSortMode => _activeSortMode;
    public InventorySortDirection ActiveSortDirection => _activeSortDirection;

    public event System.Action ExternalGridRefreshRequested;

    public void ReleaseGridForExternalUse()
    {
        _isGridExternallyOwned = true;
        InventoryGridRenderer.Clear(_spawnedCells);
        _visibleEntries.Clear();
        _selectionState.Clear();
    }

    public void RefreshAfterExternalUse()
    {
        _isGridExternallyOwned = false;
        RefreshView();
    }

    private InventoryWindowStateController _windowState;
    private InventoryUseProgressModalPresenter _useProgressModal;
    private InventoryUseProgressApplier _useProgressApplier;
    private InventoryUseCompletionService _useCompletionService;
    private IPlayerNeeds _playerNeeds;
    private readonly InventoryUseRoutineState _useRoutineState = new();
    private readonly InventorySelectionState _selectionState = new();
    private readonly HashSet<object> _openBlockOwners = new();
    private InventoryCategoryFilter _activeFilter = InventoryCategoryFilter.All;
    private readonly List<InventoryButtonBinding> _categoryFilterButtonBindings = new();
    private readonly List<InventoryButtonBinding> _sortButtonBindings = new();
    private readonly List<InventoryItemCellView> _spawnedCells = new();
    private readonly List<InventoryViewEntry> _visibleEntries = new();
    private bool _isGridExternallyOwned;
    private InventorySortMode _activeSortMode = InventorySortMode.None;
    private InventorySortDirection _activeSortDirection = InventorySortDirection.Ascending;

    public void NotifyExternalGridRefreshRequested()
    {
        ExternalGridRefreshRequested?.Invoke();
    }

    public void BlockOpenRequests(object owner)
    {
        if (owner == null)
        {
            return;
        }

        _openBlockOwners.Add(owner);
    }

    public void UnblockOpenRequests(object owner)
    {
        if (owner == null)
        {
            return;
        }

        _openBlockOwners.Remove(owner);
    }

    private bool AreOpenRequestsBlocked => _openBlockOwners.Count > 0;

    private void OnValidate()
    {
        if (_playerNeedsSource != null && _playerNeedsSource is not IPlayerNeeds)
        {
            Debug.LogWarning($"{nameof(InventoryUIController)} player needs source should implement {nameof(IPlayerNeeds)}.", this);
        }
    }
}
