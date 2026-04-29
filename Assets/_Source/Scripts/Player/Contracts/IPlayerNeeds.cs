public interface IPlayerNeeds
{
    float Thirst { get; }
    float Hunger { get; }
    float MissingThirst { get; }
    float MissingHunger { get; }

    void AddThirst(float delta);
    void AddHunger(float delta);
    float RestoreThirstUpTo(float availableHydration);
    float RestoreHungerUpTo(float availableCalories);
}
