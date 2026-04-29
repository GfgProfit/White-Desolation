public partial class PlayerNeedsController
{
    private void RefreshUI()
    {
        _presenter?.Refresh(TemperatureNormalized, FatigueNormalized, ThirstNormalized, HungerNormalized);
    }
}