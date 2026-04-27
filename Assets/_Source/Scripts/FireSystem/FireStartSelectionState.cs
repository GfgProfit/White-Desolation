using System.Collections.Generic;

public sealed class FireStartSelectionState
{
    private int _igniterIndex = -1;
    private int _tinderIndex = -1;
    private int _fuelIndex = -1;
    private int _accelerantIndex = -1;

    public void Reset(int igniterCount, int tinderCount, int fuelCount)
    {
        _igniterIndex = igniterCount > 0 ? 0 : -1;
        _tinderIndex = tinderCount > 0 ? 0 : -1;
        _fuelIndex = fuelCount > 0 ? 0 : -1;
        _accelerantIndex = -1;
    }

    public ItemData GetIgniter(IReadOnlyList<ItemData> items)
    {
        return GetSelected(items, _igniterIndex);
    }

    public ItemData GetTinder(IReadOnlyList<ItemData> items)
    {
        return GetSelected(items, _tinderIndex);
    }

    public ItemData GetFuel(IReadOnlyList<ItemData> items)
    {
        return GetSelected(items, _fuelIndex);
    }

    public ItemData GetAccelerant(IReadOnlyList<ItemData> items)
    {
        return GetSelectedOptional(items, _accelerantIndex);
    }

    public void PreviousIgniter(int count)
    {
        StepRequired(ref _igniterIndex, count, -1);
    }

    public void NextIgniter(int count)
    {
        StepRequired(ref _igniterIndex, count, 1);
    }

    public void PreviousTinder(int count)
    {
        StepRequired(ref _tinderIndex, count, -1);
    }

    public void NextTinder(int count)
    {
        StepRequired(ref _tinderIndex, count, 1);
    }

    public void PreviousFuel(int count)
    {
        StepRequired(ref _fuelIndex, count, -1);
    }

    public void NextFuel(int count)
    {
        StepRequired(ref _fuelIndex, count, 1);
    }

    public void PreviousAccelerant(int count)
    {
        StepOptional(ref _accelerantIndex, count, -1);
    }

    public void NextAccelerant(int count)
    {
        StepOptional(ref _accelerantIndex, count, 1);
    }

    private static void StepRequired(ref int index, int count, int direction)
    {
        if (count <= 0)
        {
            index = -1;
            return;
        }

        index = Mod(index + direction, count);
    }

    private static void StepOptional(ref int index, int count, int direction)
    {
        int totalStates = count + 1;
        int state = index + 1;
        state = Mod(state + direction, totalStates);
        index = state - 1;
    }

    private static ItemData GetSelected(IReadOnlyList<ItemData> items, int index)
    {
        if (items == null || index < 0 || index >= items.Count)
        {
            return null;
        }

        return items[index];
    }

    private static ItemData GetSelectedOptional(IReadOnlyList<ItemData> items, int index)
    {
        return index < 0 ? null : GetSelected(items, index);
    }

    private static int Mod(int value, int divisor)
    {
        if (divisor <= 0)
        {
            return 0;
        }

        int result = value % divisor;
        return result < 0 ? result + divisor : result;
    }
}