using System;
using UnityEngine;

[Serializable]
public sealed class CrateLootEntry
{
    [SerializeField] private ItemData _item;
    [SerializeField, Range(0f, 1f)] private float _chance = 1f;
    [SerializeField, Min(1)] private int _minCount = 1;
    [SerializeField, Min(1)] private int _maxCount = 1;

    public ItemData Item => _item;
    public float Chance => Mathf.Clamp01(_chance);
    public int MinCount => Mathf.Max(1, _minCount);
    public int MaxCount => Mathf.Max(MinCount, _maxCount);
}
