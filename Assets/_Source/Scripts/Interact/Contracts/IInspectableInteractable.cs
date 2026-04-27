public interface IInspectableInteractable
{
    bool CanInspect { get; }

    InteractionInspectInfo GetInspectInfo();

    bool TryConfirmInspectAction();
}