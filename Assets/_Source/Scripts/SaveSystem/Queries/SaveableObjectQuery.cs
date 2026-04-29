using UnityEngine;

public static class SaveableObjectQuery
{
    public static ISaveable[] FindAll()
    {
        MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        int count = CountSaveables(behaviours);
        ISaveable[] saveables = new ISaveable[count];
        int index = 0;

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is ISaveable saveable)
            {
                saveables[index] = saveable;
                index++;
            }
        }

        return saveables;
    }

    private static int CountSaveables(MonoBehaviour[] behaviours)
    {
        if (behaviours == null)
        {
            return 0;
        }

        int count = 0;

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is ISaveable)
            {
                count++;
            }
        }

        return count;
    }
}