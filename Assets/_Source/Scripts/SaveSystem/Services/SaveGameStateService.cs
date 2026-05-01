using System;

public sealed class SaveGameStateService
{
    private readonly PlayerTransformSaveService _playerTransformSaveService;
    private readonly ReferencedSaveableStateService _referencedSaveableStateService;
    private readonly SceneSaveableStateService _sceneSaveableStateService;
    private readonly SavedWorldItemSpawnService _savedWorldItemSpawnService;

    public SaveGameStateService(
        PlayerTransformSaveService playerTransformSaveService,
        ReferencedSaveableStateService referencedSaveableStateService,
        SceneSaveableStateService sceneSaveableStateService,
        SavedWorldItemSpawnService savedWorldItemSpawnService = null)
    {
        _playerTransformSaveService = playerTransformSaveService ?? throw new ArgumentNullException(nameof(playerTransformSaveService));
        _referencedSaveableStateService = referencedSaveableStateService ?? throw new ArgumentNullException(nameof(referencedSaveableStateService));
        _sceneSaveableStateService = sceneSaveableStateService ?? throw new ArgumentNullException(nameof(sceneSaveableStateService));
        _savedWorldItemSpawnService = savedWorldItemSpawnService;
    }

    public GameSaveData Capture()
    {
        GameSaveData saveData = new();

        _playerTransformSaveService.Capture(saveData);
        _referencedSaveableStateService.Capture(saveData);
        _sceneSaveableStateService.CaptureAll(saveData);

        return saveData;
    }

    public void Restore(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null)
        {
            return;
        }

        _playerTransformSaveService.Restore(saveData);
        _referencedSaveableStateService.Restore(saveData, context);
        _savedWorldItemSpawnService?.RestoreRuntimeWorldItems(saveData, context);
        _sceneSaveableStateService.RestoreAll(saveData, context);
    }
}
