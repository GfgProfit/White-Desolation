using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractController : MonoBehaviour
{
    private const float BrokenTolerance = 0.0001f;

    [Header("Raycast")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField, Min(0.1f)] private float _interactRange = 3.0f;

    [Header("Hover UI")]
    [SerializeField] private GameObject _hoverRoot;              // InteractionHolder
    [SerializeField] private CanvasGroup _hoverCanvasGroup;      // CanvasGroup на InteractionHolder
    [SerializeField] private TMP_Text _hoverNameText;            // Interaction Text

    [Header("Hover UI Extra")]
    [SerializeField] private Image _lineImage;                   // Line(Image)

    [SerializeField] private GameObject _timeHolder;             // TimeHolder
    [SerializeField] private TMP_Text _timeText;                 // child text, можно не назначать

    [SerializeField] private GameObject _temperatureHolder;      // TemperatureHolder
    [SerializeField] private TMP_Text _temperatureText;          // child text, можно не назначать

    [SerializeField] private GameObject _infoHolder;             // InfoHolder
    [SerializeField] private TMP_Text _infoText;                 // child text, можно не назначать

    [Header("Hover Fade")]
    [SerializeField, Min(0f)] private float _hoverFadeDuration = 0.15f;

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
    private IInteractionExtraInfoProvider _currentExtraInfo;
    private WorldItem _currentWorldItem;
    private WorldItem _inspectedWorldItem;

    private bool _hoverVisibleTarget;

    private bool IsInspectOpen => _inspectedWorldItem != null;

    private void Awake()
    {
        CacheHoverReferences();

        ApplyHoverInfo(InteractionHoverInfo.Empty, true);
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

    private void LateUpdate()
    {
        UpdateHoverFade();
    }

    private void CacheHoverReferences()
    {
        if (_hoverCanvasGroup == null && _hoverRoot != null)
        {
            _hoverCanvasGroup = _hoverRoot.GetComponent<CanvasGroup>();
        }

        if (_timeText == null && _timeHolder != null)
        {
            _timeText = _timeHolder.GetComponentInChildren<TMP_Text>(true);
        }

        if (_temperatureText == null && _temperatureHolder != null)
        {
            _temperatureText = _temperatureHolder.GetComponentInChildren<TMP_Text>(true);
        }

        if (_infoText == null && _infoHolder != null)
        {
            _infoText = _infoHolder.GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void UpdateCurrentTarget()
    {
        _currentInteractable = null;
        _currentHoverInfo = null;
        _currentExtraInfo = null;
        _currentWorldItem = null;

        if (_cameraTransform == null)
        {
            RefreshHover(null, null, null);
            return;
        }

        if (!Physics.Raycast(
                _cameraTransform.position,
                _cameraTransform.forward,
                out RaycastHit hit,
                _interactRange,
                _layerMask,
                QueryTriggerInteraction.Ignore))
        {
            RefreshHover(null, null, null);
            return;
        }

        _currentInteractable = hit.collider.GetComponentInParent<IInteractable>();
        _currentHoverInfo = hit.collider.GetComponentInParent<IInteractHoverInfo>();
        _currentExtraInfo = hit.collider.GetComponentInParent<IInteractionExtraInfoProvider>();
        _currentWorldItem = hit.collider.GetComponentInParent<WorldItem>();

        RefreshHover(_currentWorldItem, _currentHoverInfo, _currentExtraInfo);
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
            _currentExtraInfo = null;

            ApplyHoverInfo(InteractionHoverInfo.Empty);
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

    private void RefreshHover(
        WorldItem worldItem,
        IInteractHoverInfo hoverInfo,
        IInteractionExtraInfoProvider extraInfo)
    {
        if (IsInspectOpen)
        {
            ApplyHoverInfo(InteractionHoverInfo.Empty);
            return;
        }

        InteractionHoverInfo info = InteractionHoverInfo.Empty;

        if (worldItem != null && worldItem.ItemData != null)
        {
            info = BuildWorldItemHoverInfo(worldItem);
        }
        else if (hoverInfo != null)
        {
            info = hoverInfo.GetHoverInfo();
        }

        MergeExtraInfo(ref info, extraInfo);
        ApplyHoverInfo(info);
    }

    private static InteractionHoverInfo BuildWorldItemHoverInfo(WorldItem worldItem)
    {
        InteractionHoverInfo info = new InteractionHoverInfo
        {
            InteractionText = worldItem.ItemData.DisplayName
        };

        if (IsWorldItemBroken(worldItem))
        {
            info.InfoText = "Разрушено";
        }

        return info;
    }

    private static bool IsWorldItemBroken(WorldItem worldItem)
    {
        if (worldItem == null || worldItem.ItemData == null)
        {
            return false;
        }

        if (!worldItem.ItemData.UsesDurability || worldItem.ItemData.IsUnbreakable)
        {
            return false;
        }

        return worldItem.CurrentDurability <= BrokenTolerance;
    }

    private static void MergeExtraInfo(
        ref InteractionHoverInfo info,
        IInteractionExtraInfoProvider extraInfo)
    {
        if (extraInfo == null)
        {
            return;
        }

        if (info.HasInfoText)
        {
            return;
        }

        if (extraInfo.TryGetExtraInfo(out string extraText))
        {
            info.InfoText = extraText;
        }
    }

    private void ApplyHoverInfo(InteractionHoverInfo info, bool instant = false)
    {
        bool hasInteractionText = info.HasInteractionText;

        if (_hoverNameText != null)
        {
            _hoverNameText.text = hasInteractionText ? info.InteractionText : string.Empty;
            _hoverNameText.gameObject.SetActive(hasInteractionText);
        }

        bool hasTime = SetHolderText(_timeHolder, _timeText, info.TimeText);
        bool hasTemperature = SetHolderText(_temperatureHolder, _temperatureText, info.TemperatureText);
        bool hasInfo = SetHolderText(_infoHolder, _infoText, info.InfoText);

        bool hasExtra = hasTime || hasTemperature || hasInfo;

        if (_lineImage != null)
        {
            _lineImage.gameObject.SetActive(hasExtra);
        }

        SetHoverVisible(info.HasAnyText, instant);
    }

    private static bool SetHolderText(GameObject holder, TMP_Text text, string value)
    {
        bool visible = !string.IsNullOrWhiteSpace(value);

        if (text != null)
        {
            text.text = visible ? value : string.Empty;
        }

        if (holder != null)
        {
            holder.SetActive(visible);
        }

        return visible;
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

        if (_nameText != null)
        {
            _nameText.text = $"{worldItem.ItemData.DisplayName}";
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = $"{worldItem.ItemData.Description}";
        }

        if (_durabilityText != null)
        {
            _durabilityText.text = $"{FormatDurability(worldItem)}%";
        }

        Utils.SetDurabilityColor(worldItem, _durabilityText, _durabilityIcon);

        if (_weightText != null)
        {
            if (worldItem.CurrentWeightKg >= 1)
            {
                _weightText.text = $"{worldItem.CurrentWeightKg:0.##} кг";
            }
            else
            {
                _weightText.text = $"{worldItem.CurrentWeightKg * 1000f:0} гр";
            }
        }

        ApplyHoverInfo(InteractionHoverInfo.Empty);

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

    private void SetHoverVisible(bool visible, bool instant = false)
    {
        _hoverVisibleTarget = visible;

        if (_hoverRoot != null && visible && !_hoverRoot.activeSelf)
        {
            _hoverRoot.SetActive(true);
        }

        if (_hoverCanvasGroup == null)
        {
            if (_hoverRoot != null)
            {
                _hoverRoot.SetActive(visible);
            }

            return;
        }

        _hoverCanvasGroup.interactable = false;
        _hoverCanvasGroup.blocksRaycasts = false;

        if (!instant)
        {
            return;
        }

        _hoverCanvasGroup.alpha = visible ? 1f : 0f;

        if (_hoverRoot != null && !visible)
        {
            _hoverRoot.SetActive(false);
        }
    }

    private void UpdateHoverFade()
    {
        if (_hoverCanvasGroup == null)
        {
            return;
        }

        float targetAlpha = _hoverVisibleTarget ? 1f : 0f;

        if (_hoverFadeDuration <= 0f)
        {
            _hoverCanvasGroup.alpha = targetAlpha;
        }
        else
        {
            _hoverCanvasGroup.alpha = Mathf.MoveTowards(
                _hoverCanvasGroup.alpha,
                targetAlpha,
                Time.unscaledDeltaTime / _hoverFadeDuration);
        }

        if (_hoverRoot != null && !_hoverVisibleTarget && _hoverCanvasGroup.alpha <= 0f)
        {
            _hoverRoot.SetActive(false);
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