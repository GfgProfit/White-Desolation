using System;
using UnityEngine;

[Serializable]
public struct FireFuelStats
{
    [Min(0f)] public float BurnMinutes;
    [Range(0f, 100f)] public float StartChanceBonus;

    public FireFuelStats(float burnMinutes, float startChanceBonus)
    {
        BurnMinutes = Mathf.Max(0f, burnMinutes);
        StartChanceBonus = Mathf.Clamp(startChanceBonus, 0f, 100f);
    }
}
