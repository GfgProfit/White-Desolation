using System;
using UnityEngine;

[Serializable]
public sealed class CraftRequirement
{
    [SerializeField] private ItemData _item;
    [SerializeField, Min(1)] private int _count = 1;

    public ItemData Item => _item;
    public int Count => Mathf.Max(1, _count);
    public bool IsValid => _item != null && _count > 0;
}
