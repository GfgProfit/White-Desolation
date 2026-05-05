using System;
using System.Collections;
using UnityEngine;

public sealed partial class SaveManager
{
    public void Save()
    {
        if (_saveRoutine != null)
        {
            return;
        }

        if (!isActiveAndEnabled)
        {
            SaveImmediately();
            return;
        }

        _saveRoutine = StartCoroutine(SaveRoutine());
    }

    private IEnumerator SaveRoutine()
    {
        SetSaveStatusVisible(true);

        yield return null;

        try
        {
            SaveImmediately();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }

        SetSaveStatusVisible(false);
        _saveRoutine = null;
    }

    private void SaveImmediately()
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

    private void SetSaveStatusVisible(bool isVisible)
    {
        if (_saveStatusCanvasGroup == null)
        {
            return;
        }

        _saveStatusCanvasGroup.alpha = isVisible ? 1f : 0f;
    }
}
