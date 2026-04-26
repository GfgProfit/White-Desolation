using UnityEngine;

public readonly struct InteractionTarget
{
    public readonly bool HasHit;
    public readonly RaycastHit Hit;

    public readonly GameObject GameObject;
    public readonly Transform Transform;

    public readonly IInteractable Interactable;
    public readonly IInteractHoverInfo HoverInfo;
    public readonly IInteractionExtraInfoProvider ExtraInfo;
    public readonly IInspectableInteractable Inspectable;

    public bool HasInteractable => Interactable != null;
    public bool HasHoverInfo => HoverInfo != null;
    public bool HasExtraInfo => ExtraInfo != null;
    public bool HasInspectable => Inspectable != null && Inspectable.CanInspect;

    public InteractionTarget(RaycastHit hit, IInteractable interactable, IInteractHoverInfo hoverInfo, IInteractionExtraInfoProvider extraInfo, IInspectableInteractable inspectable)
    {
        HasHit = true;
        Hit = hit;

        GameObject = hit.collider != null ? hit.collider.gameObject : null;

        Transform = hit.collider != null ? hit.collider.transform : null;

        Interactable = interactable;
        HoverInfo = hoverInfo;
        ExtraInfo = extraInfo;
        Inspectable = inspectable;
    }

    private InteractionTarget(bool empty)
    {
        HasHit = false;
        Hit = default;

        GameObject = null;
        Transform = null;

        Interactable = null;
        HoverInfo = null;
        ExtraInfo = null;
        Inspectable = null;
    }

    public static InteractionTarget Empty => new(false);
}