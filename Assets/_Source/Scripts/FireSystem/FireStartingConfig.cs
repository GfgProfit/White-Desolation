using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Fire Starting Config", menuName = "Fire/Fire Starting Config")]
public sealed class FireStartingConfig : ScriptableObject
{
    [Header("Base Chance")]
    [SerializeField, Range(0f, 100f)] private float _baseStartChance = 50f;

    [Header("Available Item Whitelists")]
    [SerializeField] private ItemData[] _igniters;
    [SerializeField] private ItemData[] _tinders;
    [SerializeField] private ItemData[] _fuels;
    [SerializeField] private ItemData[] _accelerants;

    [Header("Optional Fuel Overrides")]
    [Tooltip("Не обязательно заполнять, если DisplayName/Id предметов совпадают с базовыми правилами ниже.")]
    [SerializeField] private FireFuelOverride[] _fuelOverrides;

    public float BaseStartChance => _baseStartChance;
    public ItemData[] Igniters => _igniters;
    public ItemData[] Tinders => _tinders;
    public ItemData[] Fuels => _fuels;
    public ItemData[] Accelerants => _accelerants;

    public FireFuelStats GetFuelStats(ItemData fuel)
    {
        if (fuel == null)
        {
            return new FireFuelStats(0f, 0f);
        }

        if (_fuelOverrides != null)
        {
            for (int i = 0; i < _fuelOverrides.Length; i++)
            {
                FireFuelOverride rule = _fuelOverrides[i];
                if (rule == null || rule.Item == null)
                {
                    continue;
                }

                if (IsSameItem(rule.Item, fuel))
                {
                    return new FireFuelStats(rule.BurnMinutes, rule.StartChanceBonus);
                }
            }
        }

        string key = $"{fuel.Id} {fuel.DisplayName}".ToLowerInvariant();

        if (ContainsAny(key, "книга", "book"))
            return new FireFuelStats(20f, 35f);

        if (ContainsAny(key, "кедр", "cedar", "елов", "spruce", "fir"))
            return new FireFuelStats(90f, 20f);

        if (ContainsAny(key, "полено", "log"))
            return new FireFuelStats(80f, 30f);

        if (ContainsAny(key, "древесина", "reclaimed", "wood"))
            return new FireFuelStats(30f, 15f);

        if (ContainsAny(key, "уголь", "coal"))
            return new FireFuelStats(100f, 25f);

        if (ContainsAny(key, "палка", "stick"))
            return new FireFuelStats(8f, 10f);

        return new FireFuelStats(0f, 0f);
    }

    private static bool IsSameItem(ItemData a, ItemData b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(a.Id) && !string.IsNullOrWhiteSpace(b.Id))
        {
            return string.Equals(a.Id, b.Id, StringComparison.Ordinal);
        }

        return ReferenceEquals(a, b);
    }

    private static bool ContainsAny(string source, params string[] parts)
    {
        if (string.IsNullOrEmpty(source) || parts == null)
        {
            return false;
        }

        for (int i = 0; i < parts.Length; i++)
        {
            if (!string.IsNullOrEmpty(parts[i]) && source.Contains(parts[i]))
            {
                return true;
            }
        }

        return false;
    }
}
