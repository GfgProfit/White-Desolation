public readonly struct InventoryCarryWeightViewModel
{
    public string CarryWeightText { get; }
    public string CurrentWeightText { get; }

    public float SliderMaxValue { get; }
    public float SliderValue { get; }

    public InventoryCarryWeightViewModel(string carryWeightText, string currentWeightText, float sliderMaxValue, float sliderValue)
    {
        CarryWeightText = carryWeightText ?? string.Empty;
        CurrentWeightText = currentWeightText ?? string.Empty;
        SliderMaxValue = sliderMaxValue;
        SliderValue = sliderValue;
    }
}