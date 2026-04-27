public partial class InteractController
{
    private void InitializeRuntimeServices()
    {
        _targetService = new InteractionTargetService(_cameraTransform, _interactRange, _layerMask);
        _hoverInfoQuery = new InteractionHoverInfoQuery();
        _inputReader = new InteractionInputReader(_playerInput);
        _inputService = new InteractionInputService();
        _inspectActionService = new InteractionInspectActionService();
        _executionService = new InteractionExecutionService();
        _inspectSession = new InteractionInspectSessionController(this, _disableWhileInspectOpen, _objectDisableWhileInspectOpen);
    }

    private void InitializeRuntimePresenters()
    {
        _hoverPresenter = new InteractionHoverPresenter(_hoverRoot, _hoverCanvasGroup, _hoverNameText, _lineImage, _timeHolder, _timeText, _temperatureHolder, _temperatureText, _infoHolder, _infoText, _hoverFadeDuration);
        _hoverPresenter.CacheReferences();

        _inspectPresenter = new InteractionInspectPresenter(_inspectRoot, _inspectIcon, _durabilityIcon, _nameText, _descriptionText, _durabilityText, _weightText);
    }

    private void InitializeRuntimeState()
    {
        ClearCurrentTarget();
        _currentInputState = InteractionInputState.Empty;

        ClearHoverInfo(true);
        _inspectPresenter?.Hide();
    }

    private void ReleaseRuntimeState()
    {
        ClearCurrentTarget();
        _currentInputState = InteractionInputState.Empty;

        _inspectSession?.Release();
        _inspectPresenter?.Hide();

        ClearHoverInfo(true);
    }
}