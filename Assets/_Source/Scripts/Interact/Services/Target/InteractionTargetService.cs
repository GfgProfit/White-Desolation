using UnityEngine;

public sealed class InteractionTargetService
{
    private InteractionRaycaster _raycaster;

    public InteractionTargetService(Transform cameraTransform, float interactRange, LayerMask layerMask)
    {
        _raycaster = new InteractionRaycaster(cameraTransform, interactRange, layerMask);
    }

    public void Configure(Transform cameraTransform, float interactRange, LayerMask layerMask)
    {
        if (_raycaster == null)
        {
            _raycaster = new InteractionRaycaster(cameraTransform, interactRange, layerMask);

            return;
        }

        _raycaster.Configure(cameraTransform, interactRange, layerMask);
    }

    public InteractionTarget GetCurrentTarget()
    {
        if (_raycaster == null)
        {
            return InteractionTarget.Empty;
        }

        if (!_raycaster.TryGetTarget(out InteractionTarget target))
        {
            return InteractionTarget.Empty;
        }

        return target;
    }
}