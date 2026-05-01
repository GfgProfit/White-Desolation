using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CraftListItemView : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private CanvasGroup _canvasGroup;

    private CraftRecipe _recipe;
    private Action<CraftRecipe> _onClicked;

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(HandleClicked);
        }
    }

    public void Bind(CraftRecipe recipe, bool canCraft, Action<CraftRecipe> onClicked)
    {
        EnsureReferences();

        _recipe = recipe;
        _onClicked = onClicked;

        if (_iconImage != null)
        {
            Sprite icon = recipe != null ? recipe.CraftListIcon : null;
            _iconImage.enabled = icon != null;
            _iconImage.sprite = icon;
        }

        if (_nameText != null)
        {
            ItemData resultItem = recipe != null ? recipe.ResultItem : null;
            string itemName = resultItem != null ? resultItem.DisplayName : string.Empty;
            int resultCount = recipe != null ? recipe.ResultCount : 0;

            _nameText.text = resultCount > 1
                ? resultItem != null ? $"{itemName} ({resultCount})" : string.Empty
                : resultItem != null ? itemName : string.Empty;
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = canCraft ? 1f : 0.2f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        if (_button != null)
        {
            _button.onClick.RemoveListener(HandleClicked);
            _button.onClick.AddListener(HandleClicked);
            _button.interactable = recipe != null;
        }
    }

    private void HandleClicked()
    {
        _onClicked?.Invoke(_recipe);
    }

    private void EnsureReferences()
    {
        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        if (_button == null)
        {
            _button = GetComponent<Button>();
        }

        if (_button == null)
        {
            _button = gameObject.AddComponent<Button>();
            _button.targetGraphic = GetComponent<Graphic>();
        }

        if (_iconImage == null)
        {
            _iconImage = FindChildComponent<Image>("Icon");
        }

        if (_nameText == null)
        {
            _nameText = FindChildComponent<TMP_Text>("Name Text");
        }
    }

    private T FindChildComponent<T>(string childName) where T : Component
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i].name == childName)
            {
                return children[i].GetComponent<T>();
            }
        }

        return null;
    }
}
