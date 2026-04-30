public sealed class ReferencedSaveableStateService
{
    private readonly ISaveableObjectProvider _saveableObjectProvider;

    public ReferencedSaveableStateService(ISaveableObjectProvider saveableObjectProvider = null)
    {
        _saveableObjectProvider = saveableObjectProvider ?? new SceneSaveableObjectProvider();
    }

    public void Capture(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        IGlobalSaveable[] saveables = _saveableObjectProvider.FindAll<IGlobalSaveable>();

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

        IGlobalSaveable[] saveables = _saveableObjectProvider.FindAll<IGlobalSaveable>();

        for (int i = 0; i < saveables.Length; i++)
        {
            saveables[i]?.RestoreState(saveData, context);
        }
    }
}
