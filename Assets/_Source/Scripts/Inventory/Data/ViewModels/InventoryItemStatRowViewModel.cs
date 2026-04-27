public readonly struct InventoryItemStatRowViewModel
{
    public bool IsVisible { get; }
    public string Text { get; }

    public InventoryItemStatRowViewModel(bool isVisible, string text)
    {
        IsVisible = isVisible;
        Text = text ?? string.Empty;
    }

    public static InventoryItemStatRowViewModel Hidden => new(false, string.Empty);
}