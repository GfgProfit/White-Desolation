using UnityEngine;

public sealed partial class SaveManager
{
    public void Save()
    {
        EnsureRuntimeServices();

        GameSaveData saveData = _gameStateService.Capture();

        _fileService.Save(_slotName, saveData);
    }

    public void Load()
    {
        EnsureRuntimeServices();

        if (!_fileService.TryLoad(_slotName, out GameSaveData saveData))
        {
            Debug.Log($"[Save] No save file for slot '{_slotName}'.");
            return;
        }

        SaveContext context = CreateSaveContext();

        _gameStateService.Restore(saveData, context);

        Debug.Log($"[Save] Loaded slot '{_slotName}'.");
    }

    public void DeleteSave()
    {
        EnsureRuntimeServices();

        _fileService.Delete(_slotName);
    }

    private SaveContext CreateSaveContext()
    {
        EnsureRuntimeServices();
        return _saveContextFactory.Create(_itemDatabaseAsset);
    }
}
