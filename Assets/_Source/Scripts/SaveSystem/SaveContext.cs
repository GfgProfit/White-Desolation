public sealed class SaveContext
{
    public readonly ItemDatabase ItemDatabase;

    public SaveContext(ItemDatabase itemDatabase)
    {
        ItemDatabase = itemDatabase;
    }
}