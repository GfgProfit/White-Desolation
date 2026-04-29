public interface IGlobalSaveable
{
    void CaptureState(GameSaveData saveData);
    void RestoreState(GameSaveData saveData, SaveContext context);
}
