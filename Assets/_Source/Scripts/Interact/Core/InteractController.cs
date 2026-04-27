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

    private InteractionTargetService _targetService;
    private InteractionHoverInfoQuery _hoverInfoQuery;
    private InteractionInputService _inputService;
    private InteractionHoverPresenter _hoverPresenter;
    private InteractionInspectPresenter _inspectPresenter;
    private InteractionInspectSessionController _inspectSession;
    private InteractionTarget _currentTarget;
    private bool IsInspectOpen => _inspectSession != null && _inspectSession.IsOpen;

    private void Awake()
    {
        _targetService = new InteractionTargetService(_cameraTransform, _interactRange, _layerMask);
        _hoverInfoQuery = new InteractionHoverInfoQuery();
        _inputService = new InteractionInputService(_playerInput);
        _hoverPresenter = new InteractionHoverPresenter(_hoverRoot, _hoverCanvasGroup, _hoverNameText, _lineImage, _timeHolder, _timeText, _temperatureHolder, _temperatureText, _infoHolder, _infoText, _hoverFadeDuration);
        _hoverPresenter.CacheReferences();
        _inspectPresenter = new InteractionInspectPresenter(_inspectRoot, _inspectIcon, _durabilityIcon, _nameText, _descriptionText, _durabilityText, _weightText);
        _inspectSession = new InteractionInspectSessionController(this, _disableWhileInspectOpen, _objectDisableWhileInspectOpen);
        ApplyHoverInfo(InteractionHoverInfo.Empty, true);
        _inspectPresenter.Hide();
    }

    private void OnValidate()
    {
        _interactRange = Mathf.Max(0.1f, _interactRange);

        _targetService?.Configure(_cameraTransform, _interactRange, _layerMask);
    }

    private void OnDisable()
    {
        _currentTarget = InteractionTarget.Empty;

        _inspectSession?.Release();
        _inspectPresenter?.Hide();

        ApplyHoverInfo(InteractionHoverInfo.Empty, true);
    }

    private void OnDestroy()
    {
        _inspectSession?.Release();
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
        if (_targetService == null)
        {
            _currentTarget = InteractionTarget.Empty;
            ApplyHoverInfo(InteractionHoverInfo.Empty);
            return;
        }

        _currentTarget = _targetService.GetCurrentTarget();

        InteractionHoverInfo hoverInfo = _hoverInfoQuery != null ? _hoverInfoQuery.Build(_currentTarget) : InteractionHoverInfo.Empty;

        ApplyHoverInfo(hoverInfo);
    }

    private bool HandleInspectableInput()
    {
        if (_inputService == null || !_inputService.TryGetInspectableTarget(_currentTarget, out IInspectableInteractable inspectable))
        {
            return false;
        }

        OpenInspection(inspectable);
        return true;
    }

    private void HandleInspectInput()
    {
        IInspectableInteractable inspectedTarget = _inspectSession?.Target;

        if (inspectedTarget == null)
        {
            CloseInspection();
            return;
        }

        InteractionInspectInputAction action = _inputService != null ? _inputService.GetInspectInputAction() : InteractionInspectInputAction.None;

        if (action == InteractionInspectInputAction.Confirm)
        {
            bool confirmed = inspectedTarget.TryConfirmInspectAction();

            if (!confirmed)
            {
                return;
            }

            CloseInspection();
            _currentTarget = InteractionTarget.Empty;
            ApplyHoverInfo(InteractionHoverInfo.Empty);
            return;
        }

        if (action == InteractionInspectInputAction.Deny)
        {
            CloseInspection();
        }
    }

    private void HandleGenericInteractableInput()
    {
        if (_inputService == null || !_inputService.TryGetGenericInteractable(_currentTarget, out IInteractable interactable))
        {
            return;
        }

        interactable.Interact();
    }

    private void ApplyHoverInfo(InteractionHoverInfo info, bool instant = false)
    {
        _hoverPresenter?.Apply(info, instant);
    }

    private void OpenInspection(IInspectableInteractable target)
    {
        if (_inspectSession == null || !_inspectSession.Open(target))
        {
            CloseInspection();
            return;
        }

        InteractionInspectInfo info = target.GetInspectInfo();

        _inspectPresenter?.Show(info);

        ApplyHoverInfo(InteractionHoverInfo.Empty);
    }

    private void CloseInspection()
    {
        _inspectSession?.Close();
        _inspectPresenter?.Hide();

        _currentTarget = InteractionTarget.Empty;
    }
}