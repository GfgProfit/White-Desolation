public partial class InteractController
{
    private void OpenInspection(IInspectableInteractable target)
    {
        if (_inspectSession == null || !_inspectSession.Open(target))
        {
            CloseInspection();
            return;
        }

        InteractionInspectInfo info = target.GetInspectInfo();

        _inspectPresenter?.Show(info);

        ApplyHoverInfo(InteractionHoverInfo.Empty);
    }

    private void CloseInspection()
    {
        _inspectSession?.Close();
        _inspectPresenter?.Hide();

        _currentTarget = InteractionTarget.Empty;
    }
}