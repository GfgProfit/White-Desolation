using System.Collections.Generic;
using UnityEngine;

public sealed class ReferencedSaveableStateService
{
    public void Capture(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        IGlobalSaveable[] saveables = FindAll();

        for (int i = 0; i < saveables.Length; i++)
        {
            saveables[i]?.CaptureState(saveData);
        }
    }

    public void Restore(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null)
        {
            return;
        }

        IGlobalSaveable[] saveables = FindAll();

        for (int i = 0; i < saveables.Length; i++)
        {
            saveables[i]?.RestoreState(saveData, context);
        }
    }

    private static IGlobalSaveable[] FindAll()
    {
        MonoBehaviour[] behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<IGlobalSaveable> saveables = new();

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] is IGlobalSaveable saveable)
            {
                saveables.Add(saveable);
            }
        }

        return saveables.ToArray();
    }
}
