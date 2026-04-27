using UnityEngine;

public sealed class InventoryUseProgressApplier
{
    private readonly PlayerNeedsController _playerNeedsController;
    private readonly float _zeroTolerance;

    private float _appliedHydration;
    private float _appliedCalories;

    public InventoryUseProgressApplier(PlayerNeedsController playerNeedsController, float zeroTolerance)
    {
        _playerNeedsController = playerNeedsController;
        _zeroTolerance = Mathf.Max(0f, zeroTolerance);
    }

    public void Reset()
    {
        _appliedHydration = 0f;
        _appliedCalories = 0f;
    }

    public void ApplyProgress(ItemUsePlan plan, float progress01)
    {
        if (plan == null)
        {
            return;
        }

        float progress = Mathf.Clamp01(progress01);

        float targetHydration = plan.HydrationToApply * progress;
        float targetCalories = plan.CaloriesToApply * progress;

        float hydrationDelta = targetHydration - _appliedHydration;
        float caloriesDelta = targetCalories - _appliedCalories;

        if (Mathf.Abs(hydrationDelta) > _zeroTolerance)
        {
            float actualHydrationDelta = ApplyHydrationDelta(hydrationDelta);
            _appliedHydration += actualHydrationDelta;
        }

        if (Mathf.Abs(caloriesDelta) > _zeroTolerance)
        {
            float actualCaloriesDelta = ApplyCaloriesDelta(caloriesDelta);
            _appliedCalories += actualCaloriesDelta;
        }
    }

    private float ApplyHydrationDelta(float hydrationDelta)
    {
        if (_playerNeedsController == null)
        {
            return 0f;
        }

        if (Mathf.Abs(hydrationDelta) <= _zeroTolerance)
        {
            return 0f;
        }

        if (hydrationDelta > 0f)
        {
            return _playerNeedsController.RestoreThirstUpTo(hydrationDelta);
        }

        float before = _playerNeedsController.Thirst;
        _playerNeedsController.AddThirst(hydrationDelta);
        return _playerNeedsController.Thirst - before;
    }

    private float ApplyCaloriesDelta(float caloriesDelta)
    {
        if (_playerNeedsController == null)
        {
            return 0f;
        }

        if (Mathf.Abs(caloriesDelta) <= _zeroTolerance)
        {
            return 0f;
        }

        if (caloriesDelta > 0f)
        {
            return _playerNeedsController.RestoreHungerUpTo(caloriesDelta);
        }

        float before = _playerNeedsController.Hunger;
        _playerNeedsController.AddHunger(caloriesDelta);
        return _playerNeedsController.Hunger - before;
    }
}