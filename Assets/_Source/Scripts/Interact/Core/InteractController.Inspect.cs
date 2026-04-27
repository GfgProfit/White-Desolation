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

        ClearHoverInfo();
    }

    private void CloseInspection()
    {
        _inspectSession?.Close();
        _inspectPresenter?.Hide();

        ClearCurrentTarget();
    }

    private void ApplyInspectActionResult(InteractionInspectActionResult result)
    {
        if (!result.ShouldClose)
        {
            return;
        }

        CloseInspection();

        if (result.ShouldClearHover)
        {
            ClearHoverInfo();
        }
    }
}