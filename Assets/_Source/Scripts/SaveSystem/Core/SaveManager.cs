using UnityEngine;
using UnityEngine.Serialization;

public sealed partial class SaveManager : MonoBehaviour
{
    [Header("Save")]
    [SerializeField] private string _slotName = "slot_0";
    [SerializeField] private bool _loadOnStart = true;
    [SerializeField] private string _serverBaseUrl = GameServerSettings.DefaultBaseUrl;

    [Header("References")]
    [FormerlySerializedAs("_itemDatabase")]
    [SerializeField] private ScriptableObject _itemDatabaseAsset;
    [SerializeField] private Transform _playerTransform;

    [Header("UI")]
    [SerializeField] private CanvasGroup _saveStatusCanvasGroup;

    [Header("World Items")]
    [SerializeField] private WorldItem _fallbackWorldItemPrefab;

    private ISaveFileService _fileService;
    private readonly SaveHotkeyInputService _hotkeyInputService = new();

    private SaveContextFactory _saveContextFactory;
    private SaveGameStateService _gameStateService;
    private Coroutine _saveRoutine;
}
