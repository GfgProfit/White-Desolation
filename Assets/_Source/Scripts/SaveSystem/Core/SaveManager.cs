using UnityEngine;
using UnityEngine.Serialization;

public sealed partial class SaveManager : MonoBehaviour
{
    [Header("Save")]
    [SerializeField] private string _slotName = "slot_0";
    [SerializeField] private bool _loadOnStart = true;

    [Header("References")]
    [FormerlySerializedAs("_itemDatabase")]
    [SerializeField] private ScriptableObject _itemDatabaseAsset;
    [SerializeField] private Transform _playerTransform;

    [Header("World Items")]
    [SerializeField] private WorldItem _fallbackWorldItemPrefab;

    private ISaveFileService _fileService;
    private readonly SaveHotkeyInputService _hotkeyInputService = new();

    private SaveContextFactory _saveContextFactory;
    private SaveGameStateService _gameStateService;
}
