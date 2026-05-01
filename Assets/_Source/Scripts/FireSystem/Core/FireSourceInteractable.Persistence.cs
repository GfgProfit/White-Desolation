using UnityEngine;

public sealed partial class FireSourceInteractable
{
    public void CaptureState(GameSaveData saveData)
    {
        if (saveData == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(SaveId))
        {
            Debug.LogWarning($"{DebugPrefix} Cannot save without SaveId: {name}");
            return;
        }

        FireSourceSaveDataCollection.RemoveBySaveId(saveData.FireSources, SaveId);

        saveData.FireSources.Add(new FireSourceSaveData
        {
            SaveId = SaveId,
            IsBurning = _isBurning,
            RemainingBurnGameMinutes = _remainingBurnGameMinutes
        });
    }

    public void RestoreState(GameSaveData saveData, SaveContext context)
    {
        if (saveData == null || saveData.FireSources == null)
        {
            return;
        }

        FireSourceSaveData state = FireSourceSaveDataCollection.FindBySaveId(saveData.FireSources, SaveId);

        if (state == null)
        {
            return;
        }

        _isBurning = state.IsBurning;
        _remainingBurnGameMinutes = Mathf.Max(0f, state.RemainingBurnGameMinutes);

        if (_remainingBurnGameMinutes <= 0f)
        {
            _isBurning = false;
        }
    }
}
