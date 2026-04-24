using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireUIController : MonoBehaviour
{
    [SerializeField] private FireStartingConfig _config;

    [Space]
    [SerializeField] private GameObject _rootFireStart;
    [SerializeField] private FireChoiceView _ignitersChoice;
    [SerializeField] private FireChoiceView _tindersChoice;
    [SerializeField] private FireChoiceView _fuelsChoice;
    [SerializeField] private FireChoiceView _accelerantsChoice;
    [SerializeField] private Button _closeButton;

    [Header("Player Lock")]
    [SerializeField] private Behaviour[] _disableWhileOpen;
    [SerializeField] private GameObject[] _objectsDisableWhileOpen;

    [Inject] private InventoryController _inventoryController;

    private List<ItemData> _availableIgniters = new();
    private List<ItemData> _availableTinders = new();
    private List<ItemData> _availableFuels = new();
    private List<ItemData> _availableAccelerants = new();

    private int _currentIgnitersIndex;
    private int _currentTindersIndex;
    private int _currentFuelsIndex;
    private int _currentAccelerantsIndex;

    private void Awake()
    {
        _ignitersChoice.Bind(
            () => StepIndex(ref _currentIgnitersIndex, _availableIgniters.Count, -1),
            () => StepIndex(ref _currentIgnitersIndex, _availableIgniters.Count, 1));

        _tindersChoice.Bind(
            () => StepIndex(ref _currentTindersIndex, _availableTinders.Count, -1),
            () => StepIndex(ref _currentTindersIndex, _availableTinders.Count, 1));

        _fuelsChoice.Bind(
            () => StepIndex(ref _currentFuelsIndex, _availableFuels.Count, -1),
            () => StepIndex(ref _currentFuelsIndex, _availableFuels.Count, 1));

        _accelerantsChoice.Bind(
            () => StepIndex(ref _currentAccelerantsIndex, _availableAccelerants.Count, -1),
            () => StepIndex(ref _currentAccelerantsIndex, _availableAccelerants.Count, 1));

        _closeButton.onClick.AddListener(() => CloseAll());
    }

    private void CloseAll()
    {
        SetPlayerControlsEnabled(true);
        SetObjectsEnabled(true);
        _rootFireStart.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void OpenFireStarting()
    {
        Debug.Log($"OpenFireStarting called. frame={Time.frameCount}, rootActive={_rootFireStart.activeSelf}");

        SetPlayerControlsEnabled(false);
        SetObjectsEnabled(false);
        ResetIndexes();

        _rootFireStart.SetActive(true);

        AddAvailableItems(_config.Igniters, _availableIgniters);
        AddAvailableItems(_config.Tinders, _availableTinders);
        AddAvailableItems(_config.Fuels, _availableFuels);
        AddAvailableItems(_config.Accelerants, _availableAccelerants);

        Refresh();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void StepIndex(ref int index, int count, int direction)
    {
        Debug.Log($"StepIndex called. index={index}, count={count}, direction={direction}, frame={Time.frameCount}");

        index += direction;

        if (index >= count)
        {
            index = 0;
        }
        else if (index < 0)
        {
            index = count - 1;
        }

        Debug.Log($"StepIndex after. index={index}");

        Refresh();
    }

    private void AddAvailableItems(ItemData[] source, List<ItemData> target)
    {
        target.Clear();

        for (int i = 0; i < source.Length; i++)
        {
            if (_inventoryController.ContainsUsableItem(source[i]))
            {
                target.Add(source[i]);
            }
        }
    }

    private void Refresh()
    {
        _ignitersChoice.Refresh(_availableIgniters.Count != 0 ? _availableIgniters[_currentIgnitersIndex] : null);
        _tindersChoice.Refresh(_availableTinders.Count != 0 ? _availableTinders[_currentTindersIndex] : null);
        _fuelsChoice.Refresh(_availableFuels.Count != 0 ? _availableFuels[_currentFuelsIndex] : null);
        _accelerantsChoice.Refresh(_availableAccelerants.Count != 0 ? _availableAccelerants[_currentAccelerantsIndex] : null);
    }

    private void ResetIndexes()
    {
        _currentIgnitersIndex = 0;
        _currentTindersIndex = 0;
        _currentFuelsIndex = 0;
        _currentAccelerantsIndex = 0;
    }

    private void SetPlayerControlsEnabled(bool enabled)
    {
        if (_disableWhileOpen == null)
        {
            return;
        }

        for (int i = 0; i < _disableWhileOpen.Length; i++)
        {
            if (_disableWhileOpen[i] != null)
            {
                _disableWhileOpen[i].enabled = enabled;
            }
        }
    }

    private void SetObjectsEnabled(bool enabled)
    {
        if (_objectsDisableWhileOpen == null)
        {
            return;
        }

        for (int i = 0; i < _objectsDisableWhileOpen.Length; i++)
        {
            if (_objectsDisableWhileOpen[i] != null)
            {
                _objectsDisableWhileOpen[i].SetActive(enabled);
            }
        }
    }
}