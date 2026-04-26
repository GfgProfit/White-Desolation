public interface ISaveable
{
    string SaveId { get; }

    void CaptureState(GameSaveData saveData);
    void RestoreState(GameSaveData saveData, SaveContext context);
}