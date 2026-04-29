public sealed partial class SaveManager
{
    private void Start()
    {
        EnsureRuntimeServices();

        if (_loadOnStart)
        {
            Load();
        }
    }

    private void Update()
    {
        if (_hotkeyInputService.IsSavePressed())
        {
            Save();
        }

        if (_hotkeyInputService.IsLoadPressed())
        {
            Load();
        }
    }
}