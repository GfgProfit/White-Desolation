public sealed class FireStartingSelection
{
    public ItemData Igniter;
    public ItemData Tinder;
    public ItemData Fuel;
    public ItemData Accelerant;

    public bool HasRequiredItems => Igniter != null && Tinder != null && Fuel != null;
    public bool UsesAccelerant => Accelerant != null;
}
