public sealed class SaveContextFactory
{
    public SaveContext Create(params object[] services)
    {
        SaveContext context = new();
        context.RegisterRange(services);

        return context;
    }
}
