public sealed class InteractionExecutionService
{
    public void Execute(IInteractable interactable)
    {
        interactable?.Interact();
    }
}