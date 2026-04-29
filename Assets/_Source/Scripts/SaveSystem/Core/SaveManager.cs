using UnityEngine;

public sealed partial class SaveManager : MonoBehaviour
{
    [Header("Save")]
    [SerializeField] private string _slotName = "slot_0";
    [SerializeField] private bool _loadOnStart = true;

    [Header("References")]
    [SerializeField] private ItemDatabase _itemDatabase;
    [SerializeField] private Transform _playerTransform;

    private readonly JsonSaveFileService _fileService = new();
    private readonly SaveHotkeyInputService _hotkeyInputService = new();

    private PlayerTransformSaveService _playerTransformSaveService;
    private SceneSaveableStateService _sceneSaveableStateService;
    private ReferencedSaveableStateService _referencedSaveableStateService;
}
