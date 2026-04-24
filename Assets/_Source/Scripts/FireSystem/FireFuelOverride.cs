using System;
using UnityEngine;

[Serializable]
public sealed class FireFuelOverride
{
    public ItemData Item;
    [Min(0f)] public float BurnMinutes;
    [Range(0f, 100f)] public float StartChanceBonus;
}
