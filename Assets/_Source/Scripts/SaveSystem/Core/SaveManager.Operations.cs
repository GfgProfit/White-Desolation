using UnityEngine;

public sealed partial class SaveManager
{
    public void Save()
    {
        GameSaveData saveData = new();

        EnsureRuntimeServices();

        _playerTransformSaveService.Capture(saveData);
        _referencedSaveableStateService.Capture(saveData);
        _sceneSaveableStateService.CaptureAll(saveData);

        _fileService.Save(_slotName, saveData);
    }

    public void Load()
    {
        if (!_fileService.TryLoad(_slotName, out GameSaveData saveData))
        {
            Debug.Log($"[Save] No save file for slot '{_slotName}'.");
            return;
        }

        EnsureRuntimeServices();

        SaveContext context = CreateSaveContext();

        _playerTransformSaveService.Restore(saveData);
        _referencedSaveableStateService.Restore(saveData, context);
        _sceneSaveableStateService.RestoreAll(saveData, context);

        Debug.Log($"[Save] Loaded slot '{_slotName}'.");
    }

    public void DeleteSave()
    {
        _fileService.Delete(_slotName);
    }

    private SaveContext CreateSaveContext()
    {
        SaveContext context = new();
        context.Register(_itemDatabaseAsset);

        return context;
    }
}
