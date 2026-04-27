using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class InteractionInspectPresenter
{
    private readonly GameObject _inspectRoot;

    private readonly Image _inspectIcon;
    private readonly Image _durabilityIcon;

    private readonly TMP_Text _nameText;
    private readonly TMP_Text _descriptionText;
    private readonly TMP_Text _durabilityText;
    private readonly TMP_Text _weightText;

    public InteractionInspectPresenter(GameObject inspectRoot, Image inspectIcon, Image durabilityIcon, TMP_Text nameText, TMP_Text descriptionText, TMP_Text durabilityText, TMP_Text weightText)
    {
        _inspectRoot = inspectRoot;
        _inspectIcon = inspectIcon;
        _durabilityIcon = durabilityIcon;
        _nameText = nameText;
        _descriptionText = descriptionText;
        _durabilityText = durabilityText;
        _weightText = weightText;
    }

    public void Show(InteractionInspectInfo info)
    {
        Apply(info);
        SetVisible(true);
    }

    public void Hide()
    {
        Clear();
        SetVisible(false);
    }

    public void Apply(InteractionInspectInfo info)
    {
        if (_inspectIcon != null)
        {
            _inspectIcon.enabled = info.Icon != null;
            _inspectIcon.sprite = info.Icon;
        }

        if (_nameText != null)
        {
            _nameText.text = info.HasName ? info.Name : string.Empty;
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = info.HasDescription ? info.Description : string.Empty;
        }

        if (_durabilityText != null)
        {
            _durabilityText.text = info.HasDurabilityText ? info.DurabilityText : string.Empty;
            _durabilityText.color = info.HasDurabilityVisual ? info.DurabilityColor : Color.white;
        }

        if (_durabilityIcon != null)
        {
            _durabilityIcon.enabled = info.HasDurabilityVisual;
            _durabilityIcon.color = info.HasDurabilityVisual ? info.DurabilityColor : Color.white;
        }

        if (_weightText != null)
        {
            _weightText.text = info.HasWeightText ? info.WeightText : string.Empty;
        }
    }

    public void Clear()
    {
        if (_inspectIcon != null)
        {
            _inspectIcon.enabled = false;
            _inspectIcon.sprite = null;
        }

        if (_nameText != null)
        {
            _nameText.text = string.Empty;
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = string.Empty;
        }

        if (_durabilityText != null)
        {
            _durabilityText.text = string.Empty;
            _durabilityText.color = Color.white;
        }

        if (_durabilityIcon != null)
        {
            _durabilityIcon.enabled = false;
            _durabilityIcon.color = Color.white;
        }

        if (_weightText != null)
        {
            _weightText.text = string.Empty;
        }
    }

    private void SetVisible(bool visible)
    {
        if (_inspectRoot != null)
        {
            _inspectRoot.SetActive(visible);
        }
    }
}