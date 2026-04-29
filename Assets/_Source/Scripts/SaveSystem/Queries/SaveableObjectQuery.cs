using UnityEngine;

public static class SaveableObjectQuery
{
    public static ISaveable[] FindAll()
    {
        return FindAll<ISaveable>();
    }

    public static T[] FindAll<T>() where T : class
    {
        MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        int count = CountSaveables<T>(behaviours);
        T[] saveables = new T[count];
        int index = 0;

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is T saveable)
            {
                saveables[index] = saveable;
                index++;
            }
        }

        return saveables;
    }

    private static int CountSaveables<T>(MonoBehaviour[] behaviours) where T : class
    {
        if (behaviours == null)
        {
            return 0;
        }

        int count = 0;

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is T)
            {
                count++;
            }
        }

        return count;
    }
}
