using UnityEngine;

public sealed class SceneSaveableStateService
{
    public void CaptureAll(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        ISaveable[] saveables = SaveableObjectQuery.FindAll();

        for (int i = 0; i < saveables.Length; i++)
        {
            ISaveable saveable = saveables[i];

            if (saveable == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(saveable.SaveId))
            {
                Debug.LogWarning($"[Save] Saveable has empty SaveId: {saveable}");
                continue;
            }

            saveable.CaptureState(saveData);
        }
    }

    public void RestoreAll(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null)
        {
            return;
        }

        ISaveable[] saveables = SaveableObjectQuery.FindAll();

        for (int i = 0; i < saveables.Length; i++)
        {
            ISaveable saveable = saveables[i];

            if (saveable == null)
            {
                continue;
            }

            saveable.RestoreState(saveData, context);
        }
    }
}