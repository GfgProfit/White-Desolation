using UnityEngine;

[CreateAssetMenu(fileName = "Fire Burning Config", menuName = "Fire System/Fire Burning Config")]
public sealed class FireBurningConfig : ScriptableObject
{
    [Header("Water")]
    [SerializeField, Min(0.01f)] private float _waterStepLiters = 0.5f;
    [SerializeField, Min(0f)] private float _meltSnowMaxLiters = 2f;
    [SerializeField, Min(0f)] private float _meltSnowGameMinutesPerStep = 30f;
    [SerializeField, Min(0f)] private float _boilWaterGameMinutesPerStep = 10f;

    public static FireBurningOperationSettings DefaultSettings => new(0.5f, 2f, 30f, 10f);

    public FireBurningOperationSettings Settings => new(_waterStepLiters, _meltSnowMaxLiters, _meltSnowGameMinutesPerStep, _boilWaterGameMinutesPerStep);
}
