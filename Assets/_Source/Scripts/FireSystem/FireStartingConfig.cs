using UnityEngine;

[CreateAssetMenu(fileName = "Fire Starting Config", menuName = "Fire System/Fire Starting Config")]
public class FireStartingConfig : ScriptableObject
{
    [SerializeField, Range(1, 100)] private float _baseChance = 50.0f;

    [Space]
    [SerializeField] private ItemData[] _igniters;
    [SerializeField] private ItemData[] _tinders;
    [SerializeField] private ItemData[] _fuels;
    [SerializeField] private ItemData[] _accelerants;

    public float BaseChance => _baseChance;
    public ItemData[] Igniters => _igniters;
    public ItemData[] Tinders => _tinders;
    public ItemData[] Fuels => _fuels;
    public ItemData[] Accelerants => _accelerants;
}