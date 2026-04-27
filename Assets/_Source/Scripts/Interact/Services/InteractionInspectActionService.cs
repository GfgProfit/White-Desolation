public sealed class InteractionInspectActionService
{
    public InteractionInspectActionResult Resolve(IInspectableInteractable inspectedTarget, InteractionInspectInputAction action)
    {
        if (inspectedTarget == null)
        {
            return InteractionInspectActionResult.Close;
        }

        if (action == InteractionInspectInputAction.Confirm)
        {
            bool confirmed = inspectedTarget.TryConfirmInspectAction();

            if (!confirmed)
            {
                return InteractionInspectActionResult.None;
            }

            return InteractionInspectActionResult.Confirmed;
        }

        if (action == InteractionInspectInputAction.Deny)
        {
            return InteractionInspectActionResult.Close;
        }

        return InteractionInspectActionResult.None;
    }
}