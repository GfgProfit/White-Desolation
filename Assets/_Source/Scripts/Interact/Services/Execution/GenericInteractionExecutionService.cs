public sealed class GenericInteractionExecutionService
{
    public void Execute(IInteractable interactable)
    {
        interactable?.Interact();
    }
}