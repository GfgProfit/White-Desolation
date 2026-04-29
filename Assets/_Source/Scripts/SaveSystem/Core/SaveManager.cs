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

    private void Start()
    {
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

    public void Save()
    {
        GameSaveData saveData = new GameSaveData();

        CapturePlayerTransform(saveData);

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

        ISaveable[] saveables = SaveableObjectQuery.FindAll();

        for (int i = 0; i < saveables.Length; i++)
        {
            ISaveable saveable = saveables[i];

            if (saveable == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(saveable.SaveId))
            {
                Debug.LogWarning($"[Save] Saveable has empty SaveId: {saveable}");
                continue;
            }

            saveable.CaptureState(saveData);
        }

        _fileService.Save(_slotName, saveData);
    }

    public void Load()
    {
        if (!_fileService.TryLoad(_slotName, out GameSaveData saveData))
        {
            Debug.Log($"[Save] No save file for slot '{_slotName}'.");
            return;
        }

        SaveContext context = new SaveContext(_itemDatabase);

        RestorePlayerTransform(saveData);

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

        ISaveable[] saveables = SaveableObjectQuery.FindAll();

        for (int i = 0; i < saveables.Length; i++)
        {
            ISaveable saveable = saveables[i];

            if (saveable == null)
            {
                continue;
            }

            saveable.RestoreState(saveData, context);
        }

        Debug.Log($"[Save] Loaded slot '{_slotName}'.");
    }

    public void DeleteSave()
    {
        _fileService.Delete(_slotName);
    }

    private void CapturePlayerTransform(GameSaveData saveData)
    {
        if (_playerTransform == null)
        {
            return;
        }

        saveData.PlayerTransform.HasData = true;
        saveData.PlayerTransform.Position = new SerializableVector3(_playerTransform.position);
        saveData.PlayerTransform.Rotation = new SerializableQuaternion(_playerTransform.rotation);
    }

    private void RestorePlayerTransform(GameSaveData saveData)
    {
        if (_playerTransform == null || saveData.PlayerTransform == null || !saveData.PlayerTransform.HasData)
        {
            return;
        }

        CharacterController characterController = _playerTransform.GetComponent<CharacterController>();

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        _playerTransform.SetPositionAndRotation(
            saveData.PlayerTransform.Position.ToVector3(),
            saveData.PlayerTransform.Rotation.ToQuaternion());

        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }
}