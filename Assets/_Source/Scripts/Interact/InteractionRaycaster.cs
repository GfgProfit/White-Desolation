using UnityEngine;

public sealed class InteractionRaycaster
{
    private Transform _origin;
    private LayerMask _layerMask;
    private float _range;

    public InteractionRaycaster(Transform origin, float range, LayerMask layerMask)
    {
        Configure(origin, range, layerMask);
    }

    public void Configure(Transform origin, float range, LayerMask layerMask)
    {
        _origin = origin;
        _range = Mathf.Max(0.1f, range);
        _layerMask = layerMask;
    }

    public bool TryGetTarget(out InteractionTarget target)
    {
        target = InteractionTarget.Empty;

        if (_origin == null)
        {
            return false;
        }

        if (!Physics.Raycast(_origin.position, _origin.forward, out RaycastHit hit, _range, _layerMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        Collider hitCollider = hit.collider;

        if (hitCollider == null)
        {
            return false;
        }

        IInteractable interactable = FindInterfaceInParent<IInteractable>(hitCollider);
        IInteractHoverInfo hoverInfo = FindInterfaceInParent<IInteractHoverInfo>(hitCollider);
        IInteractionExtraInfoProvider extraInfo = FindInterfaceInParent<IInteractionExtraInfoProvider>(hitCollider);
        IInspectableInteractable inspectable = FindInterfaceInParent<IInspectableInteractable>(hitCollider);

        target = new InteractionTarget(hit, interactable, hoverInfo, extraInfo, inspectable);

        return target.HasInteractable || target.HasHoverInfo || target.HasExtraInfo || target.HasInspectable;
    }

    private static T FindInterfaceInParent<T>(Collider source) where T : class
    {
        if (source == null)
        {
            return null;
        }

        MonoBehaviour[] behaviours = source.GetComponentsInParent<MonoBehaviour>(true);

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is T result)
            {
                return result;
            }
        }

        return null;
    }
}