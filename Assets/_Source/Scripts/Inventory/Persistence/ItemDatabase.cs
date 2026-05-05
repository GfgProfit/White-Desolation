using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Database", menuName = "Inventory/Item Database")]
public sealed class ItemDatabase : ScriptableObject
{
    private const string ResourceItemsPath = "Items";

    [SerializeField] private ItemData[] _items;

    private Dictionary<string, ItemData> _itemsById;

    public bool TryGetItem(string id, out ItemData item)
    {
        EnsureCache();

        if (string.IsNullOrWhiteSpace(id))
        {
            item = null;
            return false;
        }

        return _itemsById.TryGetValue(id, out item);
    }

    private void EnsureCache()
    {
        if (_itemsById != null)
        {
            return;
        }

        _itemsById = new Dictionary<string, ItemData>(System.StringComparer.Ordinal);

        AddItems(_items);
        AddItems(Resources.LoadAll<ItemData>(ResourceItemsPath));
    }

    private void AddItems(ItemData[] items)
    {
        if (items == null)
        {
            return;
        }

        for (int i = 0; i < items.Length; i++)
        {
            ItemData item = items[i];

            if (item == null)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.Id))
            {
                Debug.LogWarning($"[ItemDatabase] Item '{item.name}' has empty Id.");
                continue;
            }

            if (_itemsById.ContainsKey(item.Id))
            {
                continue;
            }

            _itemsById.Add(item.Id, item);
        }
    }

    private void OnEnable()
    {
        _itemsById = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _itemsById = null;
    }
#endif
}
