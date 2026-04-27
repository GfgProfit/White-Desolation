using UnityEngine;

public partial class InteractController
{
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
}