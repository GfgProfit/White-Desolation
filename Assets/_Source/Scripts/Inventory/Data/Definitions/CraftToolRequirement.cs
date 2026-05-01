using System;
using UnityEngine;

[Serializable]
public sealed class CraftToolRequirement
{
    [SerializeField] private ItemData _tool;
    [SerializeField, Min(0f)] private float _durabilityCost = 1f;

    public ItemData Tool => _tool;
    public float DurabilityCost => Mathf.Max(0f, _durabilityCost);
    public bool IsValid => _tool != null;
}
