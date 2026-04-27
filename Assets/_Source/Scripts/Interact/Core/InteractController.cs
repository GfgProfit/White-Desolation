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
    [SerializeField] private GameObject _hoverRoot;
    [SerializeField] private CanvasGroup _hoverCanvasGroup;
    [SerializeField] private TMP_Text _hoverNameText;

    [Header("Hover UI Extra")]
    [SerializeField] private Image _lineImage;

    [SerializeField] private GameObject _timeHolder;
    [SerializeField] private TMP_Text _timeText;

    [SerializeField] private GameObject _temperatureHolder;
    [SerializeField] private TMP_Text _temperatureText;

    [SerializeField] private GameObject _infoHolder;
    [SerializeField] private TMP_Text _infoText;

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

    [Inject] private IPlayerInput _playerInput;

    private InteractionRaycaster _interactionRaycaster;
    private InteractionHoverPresenter _hoverPresenter;
    private InteractionTarget _currentTarget;
    private IInspectableInteractable _inspectedTarget;
    private bool IsInspectOpen => _inspectedTarget != null;

    private void Awake()
    {
        _interactionRaycaster = new InteractionRaycaster(_cameraTransform, _interactRange, _layerMask);

        _hoverPresenter = new InteractionHoverPresenter(_hoverRoot, _hoverCanvasGroup, _hoverNameText, _lineImage, _timeHolder, _timeText, _temperatureHolder, _temperatureText, _infoHolder, _infoText, _hoverFadeDuration);

        _hoverPresenter.CacheReferences();

        ApplyHoverInfo(InteractionHoverInfo.Empty, true);
        SetInspectVisible(false);
    }

    private void OnValidate()
    {
        _interactRange = Mathf.Max(0.1f, _interactRange);

        _interactionRaycaster?.Configure(_cameraTransform, _interactRange, _layerMask);
    }

    private void OnDisable()
    {
        _currentTarget = InteractionTarget.Empty;
        _inspectedTarget = null;

        PlayerControlLockService.ReleaseOwner(this);
    }

    private void OnDestroy()
    {
        PlayerControlLockService.ReleaseOwner(this);
    }

    private void Update()
    {
        if (IsInspectOpen)
        {
            HandleInspectInput();
            return;
        }

        UpdateCurrentTarget();

        if (HandleInspectableInput())
        {
            return;
        }

        HandleGenericInteractableInput();
    }

    private void LateUpdate()
    {
        _hoverPresenter?.UpdateFade();
    }

    private void UpdateCurrentTarget()
    {
        _currentTarget = InteractionTarget.Empty;

        EnsureRaycaster();

        if (!_interactionRaycaster.TryGetTarget(out InteractionTarget target))
        {
            RefreshHover(InteractionTarget.Empty);
            return;
        }

        _currentTarget = target;
        RefreshHover(_currentTarget);
    }

    private void EnsureRaycaster()
    {
        if (_interactionRaycaster == null)
        {
            _interactionRaycaster = new InteractionRaycaster(_cameraTransform, _interactRange, _layerMask);
            return;
        }

        _interactionRaycaster.Configure(_cameraTransform, _interactRange, _layerMask);
    }

    private bool HandleInspectableInput()
    {
        if (_playerInput == null || !_playerInput.IsInteractPressed())
        {
            return false;
        }

        if (!_currentTarget.HasInspectable)
        {
            return false;
        }

        OpenInspection(_currentTarget.Inspectable);
        return true;
    }

    private void HandleInspectInput()
    {
        if (_playerInput == null)
        {
            return;
        }

        if (_inspectedTarget == null)
        {
            CloseInspection();
            return;
        }

        if (_playerInput.IsInteractPressed())
        {
            bool confirmed = _inspectedTarget.TryConfirmInspectAction();

            if (!confirmed)
            {
                return;
            }

            CloseInspection();

            _currentTarget = InteractionTarget.Empty;
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

        if (_currentTarget.HasInspectable)
        {
            return;
        }

        _currentTarget.Interactable?.Interact();
    }

    private void RefreshHover(InteractionTarget target)
    {
        if (IsInspectOpen)
        {
            ApplyHoverInfo(InteractionHoverInfo.Empty);
            return;
        }

        InteractionHoverInfo info = InteractionHoverInfo.Empty;

        if (target.HasHoverInfo)
        {
            info = target.HoverInfo.GetHoverInfo();
        }

        MergeExtraInfo(ref info, target.ExtraInfo);
        ApplyHoverInfo(info);
    }

    private static void MergeExtraInfo(ref InteractionHoverInfo info, IInteractionExtraInfoProvider extraInfo)
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
        _hoverPresenter?.Apply(info, instant);
    }

    private void OpenInspection(IInspectableInteractable target)
    {
        if (target == null || !target.CanInspect)
        {
            CloseInspection();
            return;
        }

        _inspectedTarget = target;

        InteractionInspectInfo info = target.GetInspectInfo();

        if (_inspectIcon != null)
        {
            _inspectIcon.enabled = info.Icon != null;
            _inspectIcon.sprite = info.Icon;
        }

        if (_nameText != null)
        {
            _nameText.text = info.HasName ? info.Name : string.Empty;
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = info.HasDescription ? info.Description : string.Empty;
        }

        if (_durabilityText != null)
        {
            _durabilityText.text = info.HasDurabilityText ? info.DurabilityText : string.Empty;
            _durabilityText.color = info.HasDurabilityVisual ? info.DurabilityColor : Color.white;
        }

        if (_durabilityIcon != null)
        {
            _durabilityIcon.enabled = info.HasDurabilityVisual;
            _durabilityIcon.color = info.HasDurabilityVisual ? info.DurabilityColor : Color.white;
        }

        if (_weightText != null)
        {
            _weightText.text = info.HasWeightText ? info.WeightText : string.Empty;
        }

        ApplyHoverInfo(InteractionHoverInfo.Empty);

        SetPlayerControlsEnabled(false);
        SetObjectsEnabled(false);
        SetInspectVisible(true);
    }

    private void CloseInspection()
    {
        _inspectedTarget = null;

        if (_inspectIcon != null)
        {
            _inspectIcon.enabled = false;
            _inspectIcon.sprite = null;
        }

        if (_nameText != null)
        {
            _nameText.text = string.Empty;
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = string.Empty;
        }

        if (_durabilityText != null)
        {
            _durabilityText.text = string.Empty;
            _durabilityText.color = Color.white;
        }

        if (_durabilityIcon != null)
        {
            _durabilityIcon.enabled = false;
            _durabilityIcon.color = Color.white;
        }

        if (_weightText != null)
        {
            _weightText.text = string.Empty;
        }

        _currentTarget = InteractionTarget.Empty;

        SetInspectVisible(false);
        SetPlayerControlsEnabled(true);
        SetObjectsEnabled(true);
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
        if (enabled)
        {
            PlayerControlLockService.UnlockBehaviours(this, _disableWhileInspectOpen);
        }
        else
        {
            PlayerControlLockService.LockBehaviours(this, _disableWhileInspectOpen);
        }
    }

    private void SetObjectsEnabled(bool enabled)
    {
        if (enabled)
        {
            PlayerControlLockService.UnlockGameObjects(this, _objectDisableWhileInspectOpen);
        }
        else
        {
            PlayerControlLockService.LockGameObjects(this, _objectDisableWhileInspectOpen);
        }
    }
}