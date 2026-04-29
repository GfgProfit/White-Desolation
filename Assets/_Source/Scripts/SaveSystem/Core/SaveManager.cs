using UnityEngine;

public sealed class SaveManager : MonoBehaviour
{
    [Header("Save")]
    [SerializeField] private string _slotName = "slot_0";
    [SerializeField] private bool _loadOnStart = true;

    [Header("References")]
    [SerializeField] private ItemDatabase _itemDatabase;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private InventoryController _inventoryController;
    [SerializeField] private PlayerNeedsController _playerNeedsController;
    [SerializeField] private DayNightCycle _dayNightCycle;

    private readonly JsonSaveFileService _fileService = new();

    private PlayerTransformSaveService _playerTransformSaveService;
    private SceneSaveableStateService _sceneSaveableStateService;

    private void Start()
    {
        _playerTransformSaveService = new PlayerTransformSaveService(_playerTransform);

        if (_loadOnStart)
        {
            Load();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Save();
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            Load();
        }
    }

    private void EnsureRuntimeServices()
    {
        _playerTransformSaveService ??= new PlayerTransformSaveService(_playerTransform);
        _sceneSaveableStateService ??= new SceneSaveableStateService();
    }

    public void Save()
    {
        GameSaveData saveData = new();

        EnsureRuntimeServices();

        _playerTransformSaveService.Capture(saveData);

        if (_inventoryController != null)
        {
            _inventoryController.CaptureState(saveData);
        }

        if (_playerNeedsController != null)
        {
            _playerNeedsController.CaptureState(saveData);
        }

        if (_dayNightCycle != null)
        {
            _dayNightCycle.CaptureState(saveData);
        }

        _sceneSaveableStateService.CaptureAll(saveData);

        _fileService.Save(_slotName, saveData);
    }

    public void Load()
    {
        if (!_fileService.TryLoad(_slotName, out GameSaveData saveData))
        {
            Debug.Log($"[Save] No save file for slot '{_slotName}'.");
            return;
        }

        EnsureRuntimeServices();

        SaveContext context = new(_itemDatabase);

        _playerTransformSaveService.Restore(saveData);

        if (_inventoryController != null)
        {
            _inventoryController.RestoreState(saveData, context);
        }

        if (_playerNeedsController != null)
        {
            _playerNeedsController.RestoreState(saveData, context);
        }

        if (_dayNightCycle != null)
        {
            _dayNightCycle.RestoreState(saveData, context);
        }

        _sceneSaveableStateService.RestoreAll(saveData, context);

        Debug.Log($"[Save] Loaded slot '{_slotName}'.");
    }

    public void DeleteSave()
    {
        _fileService.Delete(_slotName);
    }
}