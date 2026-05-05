public sealed partial class SaveManager
{
    private void EnsureRuntimeServices()
    {
        _fileService ??= new ServerSaveFileService(_serverBaseUrl);
        _saveContextFactory ??= new SaveContextFactory();

        if (_gameStateService != null)
        {
            return;
        }

        ISaveableObjectProvider saveableObjectProvider = new SceneSaveableObjectProvider();
        PlayerTransformSaveService playerTransformSaveService = new(_playerTransform);
        ReferencedSaveableStateService referencedSaveableStateService = new(saveableObjectProvider);
        SceneSaveableStateService sceneSaveableStateService = new(saveableObjectProvider);
        SavedWorldItemSpawnService savedWorldItemSpawnService = new(saveableObjectProvider, _fallbackWorldItemPrefab);

        _gameStateService = new SaveGameStateService(
            playerTransformSaveService,
            referencedSaveableStateService,
            sceneSaveableStateService,
            savedWorldItemSpawnService);
    }
}
