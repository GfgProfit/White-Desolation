public partial class InteractController
{
    private void UpdateCurrentTarget()
    {
        if (_targetService == null)
        {
            _currentTarget = InteractionTarget.Empty;
            ApplyHoverInfo(InteractionHoverInfo.Empty);
            return;
        }

        _currentTarget = _targetService.GetCurrentTarget();

        InteractionHoverInfo hoverInfo = _hoverInfoQuery != null ? _hoverInfoQuery.Build(_currentTarget) : InteractionHoverInfo.Empty;

        ApplyHoverInfo(hoverInfo);
    }

    private void ApplyHoverInfo(InteractionHoverInfo info, bool instant = false)
    {
        _hoverPresenter?.Apply(info, instant);
    }
}