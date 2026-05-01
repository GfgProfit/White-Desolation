public sealed partial class CrateContainer
{
    private void EnsureLootGenerated()
    {
        if (_lootGenerated)
        {
            return;
        }

        _lootGenerated = true;

        bool generatedItems = CrateLootGenerationService.TryGenerate(
            _items,
            _lootTable,
            _maxGeneratedItemCount,
            _maxWeightKg);

        if (generatedItems)
        {
            NotifyChanged();
        }
    }
}
