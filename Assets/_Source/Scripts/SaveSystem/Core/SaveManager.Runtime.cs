public sealed partial class SaveManager
{
    private void EnsureRuntimeServices()
    {
        _saveContextFactory ??= new SaveContextFactory();

        if (_gameStateService != null)
        {
            return;
        }

        ISaveableObjectProvider saveableObjectProvider = new SceneSaveableObjectProvider();
        PlayerTransformSaveService playerTransformSaveService = new(_playerTransform);
        ReferencedSaveableStateService referencedSaveableStateService = new(saveableObjectProvider);
        SceneSaveableStateService sceneSaveableStateService = new(saveableObjectProvider);

        _gameStateService = new SaveGameStateService(
            playerTransformSaveService,
            referencedSaveableStateService,
            sceneSaveableStateService);
    }
}
