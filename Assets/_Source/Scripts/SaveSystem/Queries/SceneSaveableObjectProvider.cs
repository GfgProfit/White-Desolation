public sealed class SceneSaveableObjectProvider : ISaveableObjectProvider
{
    public T[] FindAll<T>() where T : class
    {
        return SaveableObjectQuery.FindAll<T>();
    }
}
