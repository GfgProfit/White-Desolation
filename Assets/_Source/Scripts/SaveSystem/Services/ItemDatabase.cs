using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Database", menuName = "Save System/Item Database")]
public sealed class ItemDatabase : ScriptableObject
{
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

        _itemsById = new Dictionary<string, ItemData>();

        if (_items == null)
        {
            return;
        }

        for (int i = 0; i < _items.Length; i++)
        {
            ItemData item = _items[i];

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
                Debug.LogWarning($"[ItemDatabase] Duplicate item id: {item.Id}");
                continue;
            }

            _itemsById.Add(item.Id, item);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _itemsById = null;
    }
#endif
}