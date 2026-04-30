using System.Collections.Generic;
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
        List<T> saveables = new();

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is T saveable)
            {
                saveables.Add(saveable);
            }
        }

        return saveables.ToArray();
    }
}
