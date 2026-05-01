using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CrateWindowPresenter
{
    private const float WeightWarningOffsetKg = 5f;

    private readonly Button _actionButton;
    private readonly TMP_Text _weightText;
    private readonly Slider _weightSlider;

    public CrateWindowPresenter(Button actionButton, TMP_Text weightText, Slider weightSlider)
    {
        _actionButton = actionButton;
        _weightText = weightText;
        _weightSlider = weightSlider;
    }

    public void RefreshWeight(CrateContainer crate)
    {
        if (crate == null)
        {
            return;
        }

        float currentWeight = crate.CurrentWeightKg;
        float maxWeight = crate.MaxWeightKg;

        if (_weightText != null)
        {
            _weightText.text = FormatCrateWeight(currentWeight, maxWeight);
        }

        if (_weightSlider != null)
        {
            _weightSlider.maxValue = maxWeight;
            _weightSlider.value = currentWeight;
        }
    }

    public void RefreshActionButton(CrateSelectionSource selectionSource, bool hasSelection)
    {
        if (_actionButton == null)
        {
            return;
        }

        _actionButton.interactable = hasSelection;

        Transform actionTransform = _actionButton.transform;
        Vector3 eulerAngles = actionTransform.localEulerAngles;
        eulerAngles.z = selectionSource == CrateSelectionSource.CrateInventory ? 180f : 0f;
        actionTransform.localEulerAngles = eulerAngles;
    }

    private static string FormatCrateWeight(float currentWeightKg, float maxWeightKg)
    {
        if (maxWeightKg <= 0f)
        {
            return $"{currentWeightKg:0.##} КГ";
        }

        string maxText = $"{maxWeightKg:0.##}";

        if (currentWeightKg >= Mathf.Max(0f, maxWeightKg - WeightWarningOffsetKg))
        {
            maxText = $"<color=#9E2F3C>{maxText}</color>";
        }

        return $"{currentWeightKg:0.##} / {maxText} КГ";
    }
}
