public interface ISaveFileService
{
    string GetSavePath(string slotName);
    void Save(string slotName, GameSaveData saveData);
    bool TryLoad(string slotName, out GameSaveData saveData);
    bool Exists(string slotName);
    void Delete(string slotName);
}
