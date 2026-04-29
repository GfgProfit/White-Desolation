public sealed partial class SaveManager
{
    private void EnsureRuntimeServices()
    {
        _playerTransformSaveService ??= new PlayerTransformSaveService(_playerTransform);
        _sceneSaveableStateService ??= new SceneSaveableStateService();
        _referencedSaveableStateService ??= new ReferencedSaveableStateService();
    }
}
