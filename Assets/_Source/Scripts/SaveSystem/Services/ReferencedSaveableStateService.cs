public sealed class ReferencedSaveableStateService
{
    public void Capture(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        IGlobalSaveable[] saveables = SaveableObjectQuery.FindAll<IGlobalSaveable>();

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

        IGlobalSaveable[] saveables = SaveableObjectQuery.FindAll<IGlobalSaveable>();

        for (int i = 0; i < saveables.Length; i++)
        {
            saveables[i]?.RestoreState(saveData, context);
        }
    }

}
