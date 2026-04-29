public partial class FireUIController : IFireSourceInteractionHandler
{
    public void InteractWith(FireSourceInteractable source)
    {
        if (source == null)
        {
            return;
        }

        if (source.IsBurning)
        {
            OpenBurningFire(source);
            return;
        }

        OpenFireStarting(source);
    }
}
