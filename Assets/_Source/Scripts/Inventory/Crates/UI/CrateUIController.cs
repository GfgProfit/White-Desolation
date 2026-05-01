using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed partial class CrateUIController : MonoBehaviour
{
    [Header("Auto References")]
    [SerializeField] private InventoryUIController _inventoryUIController;
    [SerializeField] private InteractController _interactController;
    [SerializeField] private GameObject _inventoryRightPanel;
    [SerializeField] private GameObject _rightCratePanel;

    [Header("Search Progress")]
    [SerializeField] private GameObject _searchProgressRoot;
    [SerializeField] private Image _searchProgressFillImage;
    [SerializeField] private TMP_Text _searchProgressText;
    [SerializeField] private string _searchProgressLabel = "Поиск";

    [Header("Take Item Window")]
    [SerializeField] private GameObject _takeItemRoot;
    [SerializeField] private Image _takeItemIcon;
    [SerializeField] private Image _takeItemDurabilityIcon;
    [SerializeField] private TMP_Text _takeItemNameText;
    [SerializeField] private TMP_Text _takeItemDescriptionText;
    [SerializeField] private TMP_Text _takeItemDurabilityText;
    [SerializeField] private TMP_Text _takeItemWeightText;

    [Header("Crate Window")]
    [SerializeField] private GameObject _crateRoot;
    [SerializeField] private Transform _playerGridRoot;
    [SerializeField] private Transform _crateGridRoot;
    [SerializeField] private InventoryItemCellView _cellPrefab;
    [SerializeField] private Button _crateActionButton;
    [SerializeField] private TMP_Text _crateWeightText;
    [SerializeField] private Slider _crateWeightSlider;

    [Header("Optional")]
    [SerializeField] private Behaviour[] _disableWhileOpen;
    [SerializeField] private GameObject[] _objectDisableWhileOpen;

    [Inject] private InventoryController _inventoryController = null;
    [Inject] private IPlayerInput _playerInput = null;

    private readonly List<InventoryItemCellView> _playerCells = new();
    private readonly List<InventoryItemCellView> _crateCells = new();
    private readonly List<InventoryViewEntry> _playerEntries = new();
    private readonly List<InventoryViewEntry> _crateEntries = new();
    private readonly List<InventorySlot> _searchedSlots = new();
    private readonly InventorySelectionState _playerSelection = new();
    private readonly InventorySelectionState _crateSelection = new();

    private InventoryUseProgressModalPresenter _searchProgress;
    private InteractionInspectPresenter _takeItemPresenter;
    private CrateWindowPresenter _crateWindowPresenter;
    private CrateContainer _activeCrate;
    private Coroutine _searchRoutine;
    private int _searchedSlotCursor;
    private CrateSelectionSource _selectionSource;
    private bool _isCrateOpen;
    private bool _isBrowsingSearchResults;
    private bool _hasCrateWindowSnapshot;
    private bool _crateRootWasActive;
    private bool _rightCratePanelWasActive;
    private bool _inventoryRightPanelWasActive;
    private bool _inventoryUIControllerWasEnabled;

    private void Awake()
    {
        EnsureRuntimeReferences();
        AutoWireSceneReferences();
        InitializePresenters();
        InitializeWindowState();

        if (_crateActionButton != null)
        {
            _crateActionButton.onClick.AddListener(HandleCrateActionClicked);
        }
    }

    private void OnDestroy()
    {
        if (_crateActionButton != null)
        {
            _crateActionButton.onClick.RemoveListener(HandleCrateActionClicked);
        }

        StopSearchRoutine();
        CloseCrate();
        CloseBrowsing();

        InventoryGridRenderer.Clear(_playerCells);
        InventoryGridRenderer.Clear(_crateCells);
    }

    private void Update()
    {
        EnsureRuntimeReferences();

        if (_isBrowsingSearchResults)
        {
            HandleBrowseInput();
            return;
        }

        if (_isCrateOpen && _playerInput != null && (_playerInput.IsEscapePressed() || _playerInput.IsInventoryPressed()))
        {
            CloseCrate();
        }
    }

    private void InitializePresenters()
    {
        _searchProgress = new InventoryUseProgressModalPresenter(_searchProgressRoot, _searchProgressFillImage, _searchProgressText);
        _takeItemPresenter = new InteractionInspectPresenter(_takeItemRoot, _takeItemIcon, _takeItemDurabilityIcon, _takeItemNameText, _takeItemDescriptionText, _takeItemDurabilityText, _takeItemWeightText);
        _crateWindowPresenter = new CrateWindowPresenter(_crateActionButton, _crateWeightText, _crateWeightSlider);

        _searchProgress.InitializeHidden();
        _takeItemPresenter.Hide();
    }

    private void InitializeWindowState()
    {
        if (_rightCratePanel != null)
        {
            _rightCratePanel.SetActive(false);
        }
    }
}
