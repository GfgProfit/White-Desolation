using UnityEngine;

public partial class InteractController
{
    private void Awake()
    {
        InitializeRuntimeServices();
        InitializeRuntimePresenters();
        InitializeRuntimeState();
    }

    private void OnValidate()
    {
        _interactRange = Mathf.Max(0.1f, _interactRange);
        _targetService?.Configure(_cameraTransform, _interactRange, _layerMask);
    }

    private void OnDisable()
    {
        ReleaseRuntimeState();
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