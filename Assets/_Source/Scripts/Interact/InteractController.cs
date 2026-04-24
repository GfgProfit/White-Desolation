using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractController : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField, Min(0.1f)] private float _interactRange = 3.0f;

    [Header("Hover UI")]
    [SerializeField] private GameObject _hoverRoot;
    [SerializeField] private TMP_Text _hoverNameText;

    [Header("Inspect UI")]
    [SerializeField] private GameObject _inspectRoot;
    [SerializeField] private Image _inspectIcon;
    [SerializeField] private Image _durabilityIcon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _durabilityText;
    [SerializeField] private TMP_Text _weightText;

    [Header("Optional")]
    [SerializeField] private Behaviour[] _disableWhileInspectOpen;
    [SerializeField] private GameObject[] _objectDisableWhileInspectOpen;

    [Inject] private readonly IPlayerInput _playerInput;

    private IInteractable _currentInteractable;
    private IInteractHoverInfo _currentHoverInfo;
    private WorldItem _currentWorldItem;
    private WorldItem _inspectedWorldItem;

    private bool IsInspectOpen => _inspectedWorldItem != null;

    private void Awake()
    {
        SetHoverVisible(false);
        SetInspectVisible(false);
    }

    private void Update()
    {
        if (IsInspectOpen)
        {
            HandleInspectInput();
            return;
        }

        UpdateCurrentTarget();
        HandleWorldItemInput();
        HandleGenericInteractableInput();
    }

    private void UpdateCurrentTarget()
    {
        _currentInteractable = null;
        _currentHoverInfo = null;
        _currentWorldItem = null;

        if (_cameraTransform == null)
        {
            RefreshHover(null, null);
            return;
        }

        if (!Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, _interactRange, _layerMask, QueryTriggerInteraction.Ignore))
        {
            RefreshHover(null, null);
            return;
        }

        _currentInteractable = hit.collider.GetComponentInParent<IInteractable>();
        _currentHoverInfo = hit.collider.GetComponentInParent<IInteractHoverInfo>();
        _currentWorldItem = hit.collider.GetComponentInParent<WorldItem>();

        RefreshHover(_currentWorldItem, _currentHoverInfo);
    }

    private void HandleWorldItemInput()
    {
        if (_playerInput == null || !_playerInput.IsInteractPressed())
        {
            return;
        }

        if (_currentWorldItem == null)
        {
            return;
        }

        OpenInspection(_currentWorldItem);
    }

    private void HandleInspectInput()
    {
        if (_playerInput == null)
        {
            return;
        }

        if (_inspectedWorldItem == null)
        {
            CloseInspection();
            return;
        }

        if (_playerInput.IsInteractPressed())
        {
            bool pickedUp = _inspectedWorldItem.TryPickup();
            if (!pickedUp)
            {
                return;
            }

            CloseInspection();
            _currentWorldItem = null;
            _currentInteractable = null;
            _currentHoverInfo = null;
            RefreshHover(null, null);
            return;
        }

        if (_playerInput.IsInteractDenied())
        {
            CloseInspection();
        }
    }

    private void HandleGenericInteractableInput()
    {
        if (_playerInput == null || !_playerInput.IsInteractPressed())
        {
            return;
        }

        if (_currentWorldItem != null)
        {
            return;
        }

        _currentInteractable?.Interact();
    }

    private void RefreshHover(WorldItem worldItem, IInteractHoverInfo hoverInfo)
    {
        if (IsInspectOpen)
        {
            SetHoverVisible(false);
            return;
        }

        if (worldItem != null && worldItem.ItemData != null)
        {
            if (_hoverNameText != null)
            {
                _hoverNameText.text = worldItem.ItemData.DisplayName;
            }

            SetHoverVisible(true);
            return;
        }

        if (hoverInfo != null)
        {
            string hoverText = hoverInfo.GetHoverText();
            if (!string.IsNullOrWhiteSpace(hoverText))
            {
                if (_hoverNameText != null)
                {
                    _hoverNameText.text = hoverText;
                }

                SetHoverVisible(true);
                return;
            }
        }

        SetHoverVisible(false);
    }

    private void OpenInspection(WorldItem worldItem)
    {
        if (worldItem == null || worldItem.ItemData == null)
        {
            CloseInspection();
            return;
        }

        _inspectedWorldItem = worldItem;

        if (_inspectIcon != null)
        {
            _inspectIcon.enabled = worldItem.ItemData.Icon != null;
            _inspectIcon.sprite = worldItem.ItemData.Icon;
        }

        _nameText.text = $"{worldItem.ItemData.DisplayName}";
        _descriptionText.text = $"{worldItem.ItemData.Description}";
        _durabilityText.text = $"{FormatDurability(worldItem)}%";
        Utils.SetDurabilityColor(worldItem, _durabilityText, _durabilityIcon);

        if (worldItem.CurrentWeightKg >= 1)
        {
            _weightText.text = $"{worldItem.CurrentWeightKg:0.##} кг";
        }
        else
        {
            _weightText.text = $"{worldItem.CurrentWeightKg * 1000f:0} гр";
        }

        SetHoverVisible(false);
        SetPlayerControlsEnabled(false);
        SetObjectsEnabled(false);
        SetInspectVisible(true);
    }

    private void CloseInspection()
    {
        _inspectedWorldItem = null;

        if (_inspectIcon != null)
        {
            _inspectIcon.enabled = false;
            _inspectIcon.sprite = null;
        }

        if (_nameText != null)
        {
            _nameText.text = string.Empty;
        }

        SetInspectVisible(false);
        SetPlayerControlsEnabled(true);
        SetObjectsEnabled(true);
    }

    private static string FormatDurability(WorldItem worldItem)
    {
        if (worldItem == null || worldItem.ItemData == null)
        {
            return "—";
        }

        if (!worldItem.HasDurability)
        {
            return "—";
        }

        if (worldItem.ItemData.IsUnbreakable)
        {
            return "Неразрушаемый";
        }

        return $"{worldItem.CurrentDurability:0.##}";
    }

    private void SetHoverVisible(bool visible)
    {
        if (_hoverRoot != null)
        {
            _hoverRoot.SetActive(visible);
        }
    }

    private void SetInspectVisible(bool visible)
    {
        if (_inspectRoot != null)
        {
            _inspectRoot.SetActive(visible);
        }
    }

    private void SetPlayerControlsEnabled(bool enabled)
    {
        if (_disableWhileInspectOpen == null)
        {
            return;
        }

        for (int i = 0; i < _disableWhileInspectOpen.Length; i++)
        {
            if (_disableWhileInspectOpen[i] != null)
            {
                _disableWhileInspectOpen[i].enabled = enabled;
            }
        }
    }

    private void SetObjectsEnabled(bool enabled)
    {
        if (_objectDisableWhileInspectOpen == null)
        {
            return;
        }

        for (int i = 0; i < _objectDisableWhileInspectOpen.Length; i++)
        {
            if (_objectDisableWhileInspectOpen[i] != null)
            {
                _objectDisableWhileInspectOpen[i].SetActive(enabled);
            }
        }
    }
}
