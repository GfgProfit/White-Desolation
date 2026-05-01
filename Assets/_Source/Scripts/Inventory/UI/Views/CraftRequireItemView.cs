using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CraftRequireItemView : MonoBehaviour
{
    private static readonly Color EnoughColor = Color.white;
    private static readonly Color MissingColor = Color.red;

    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;

    public void Bind(CraftRequirement requirement, int ownedCount)
    {
        ItemData item = requirement != null ? requirement.Item : null;
        int requiredCount = requirement != null ? requirement.Count : 0;

        Bind(item, ownedCount, requiredCount);
    }

    public void Bind(ItemData item, int ownedCount, int requiredCount)
    {
        EnsureReferences();

        bool hasEnough = ownedCount >= requiredCount;
        Color color = hasEnough ? EnoughColor : MissingColor;

        if (_iconImage != null)
        {
            Sprite icon = item != null ? item.Icon : null;
            _iconImage.enabled = icon != null;
            _iconImage.sprite = icon;
            _iconImage.color = color;
        }

        if (_nameText != null)
        {
            string itemName = item != null ? item.DisplayName : string.Empty;
            _nameText.text = $"{ownedCount}/{requiredCount} {itemName}";
            _nameText.color = color;
        }
    }

    private void EnsureReferences()
    {
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
