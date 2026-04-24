using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Fire System Config", menuName = "Fire/Fire System")]
public class FireItemsConfig : ScriptableObject
{
    [Header("Base Chance")]
    [SerializeField, Range(0f, 100f)] private float _baseStartChance = 50f;

    [Header("Available Item Whitelists")]
    [SerializeField] private ItemData[] _igniters;
    [SerializeField] private ItemData[] _tinders;
    [SerializeField] private ItemData[] _fuels;
    [SerializeField] private ItemData[] _accelerants;

    public float BaseStartChance => _baseStartChance;
    public ItemData[] Igniters => _igniters;
    public ItemData[] Tinders => _tinders;
    public ItemData[] Fuels => _fuels;
    public ItemData[] Accelerants => _accelerants;
}