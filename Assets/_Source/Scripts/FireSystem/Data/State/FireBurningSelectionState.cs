using UnityEngine;

public sealed class FireBurningSelectionState
{
    public FireBurningOperationTab Tab { get; private set; } = FireBurningOperationTab.AddFuel;
    public FireBurningWaterMode WaterMode { get; private set; } = FireBurningWaterMode.MeltSnow;
    public int SelectedIndex { get; private set; }
    public float SelectedWaterAmount { get; private set; }

    public void Reset(FireBurningOperationSettings settings)
    {
        Tab = FireBurningOperationTab.AddFuel;
        SelectedIndex = 0;
        WaterMode = FireBurningWaterMode.MeltSnow;
        ResetWaterAmount(settings, settings.MeltSnowMaxLiters);
    }

    public void SelectTab(FireBurningOperationTab tab, FireBurningOperationSettings settings)
    {
        Tab = tab;
        SelectedIndex = 0;

        if (Tab == FireBurningOperationTab.Water)
        {
            WaterMode = FireBurningWaterMode.MeltSnow;
            ResetWaterAmount(settings, settings.MeltSnowMaxLiters);
        }
    }

    public bool SelectListItem(int index, FireBurningOperationList list, FireBurningOperationSettings settings, float maxWaterAmount)
    {
        if (list == null || index < 0 || index >= list.Count || !list.IsInteractable(index))
        {
            return false;
        }

        if (Tab == FireBurningOperationTab.Water)
        {
            WaterMode = index == 1 ? FireBurningWaterMode.BoilWater : FireBurningWaterMode.MeltSnow;
            SelectedIndex = index;
            ResetWaterAmount(settings, maxWaterAmount);

            return true;
        }

        SelectedIndex = index;

        return true;
    }

    public void Clamp(FireBurningOperationList list, FireBurningOperationSettings settings, float maxWaterAmount)
    {
        if (Tab == FireBurningOperationTab.Water)
        {
            if (WaterMode == FireBurningWaterMode.BoilWater && (list == null || list.Count < 2 || !list.IsInteractable(1)))
            {
                WaterMode = FireBurningWaterMode.MeltSnow;
                ResetWaterAmount(settings, settings.MeltSnowMaxLiters);
                maxWaterAmount = settings.MeltSnowMaxLiters;
            }

            SelectedIndex = WaterMode == FireBurningWaterMode.BoilWater ? 1 : 0;
            ClampWaterAmount(settings, maxWaterAmount);
            return;
        }

        if (list == null || list.Count == 0)
        {
            SelectedIndex = 0;
            return;
        }

        SelectedIndex = Mathf.Clamp(SelectedIndex, 0, list.Count - 1);
    }

    public void DecreaseWaterAmount(FireBurningOperationSettings settings, float maxWaterAmount)
    {
        if (Tab != FireBurningOperationTab.Water)
        {
            return;
        }

        float minAmount = settings.GetMinWaterAmount(maxWaterAmount);

        if (SelectedWaterAmount <= minAmount)
        {
            return;
        }

        float previousAmount = Mathf.Floor((SelectedWaterAmount - 0.0001f) / settings.WaterStepLiters) * settings.WaterStepLiters;
        SelectedWaterAmount = settings.RoundAmount(Mathf.Max(minAmount, previousAmount));
    }

    public void IncreaseWaterAmount(FireBurningOperationSettings settings, float maxWaterAmount)
    {
        if (Tab != FireBurningOperationTab.Water)
        {
            return;
        }

        if (SelectedWaterAmount >= maxWaterAmount)
        {
            return;
        }

        SelectedWaterAmount = settings.RoundAmount(Mathf.Min(maxWaterAmount, SelectedWaterAmount + settings.WaterStepLiters));
    }

    public bool CanDecreaseWaterAmount(FireBurningOperationSettings settings, float maxWaterAmount)
    {
        return SelectedWaterAmount > settings.GetMinWaterAmount(maxWaterAmount);
    }

    public bool CanIncreaseWaterAmount(float maxWaterAmount)
    {
        return SelectedWaterAmount < maxWaterAmount;
    }

    private void ResetWaterAmount(FireBurningOperationSettings settings, float maxWaterAmount)
    {
        SelectedWaterAmount = settings.RoundAmount(settings.GetMinWaterAmount(maxWaterAmount));
    }

    private void ClampWaterAmount(FireBurningOperationSettings settings, float maxWaterAmount)
    {
        if (maxWaterAmount <= 0f)
        {
            SelectedWaterAmount = 0f;
            return;
        }

        float minAmount = settings.GetMinWaterAmount(maxWaterAmount);
        SelectedWaterAmount = settings.RoundAmount(Mathf.Clamp(SelectedWaterAmount, minAmount, maxWaterAmount));
    }
}
