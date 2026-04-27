using System;
using System.Collections.Generic;
using UnityEngine;

public partial class InventoryController : MonoBehaviour
{
    private const float ZeroTolerance = 0.0001f;

    [Header("Weight Limit")]
    [SerializeField, Min(0f)] private float _maxCarryWeightKg = 30f;

    private readonly List<InventorySlot> _items = new();

    public IReadOnlyList<InventorySlot> Items => _items;
    public int SlotCount => _items.Count;
    public float MaxCarryWeightKg => _maxCarryWeightKg;
    public float CurrentCarryWeightKg => GetCurrentTotalWeightKg();

    public event Action OnInventoryChanged;
}