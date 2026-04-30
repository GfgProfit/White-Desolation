public interface ISaveableObjectProvider
{
    T[] FindAll<T>() where T : class;
}
