using UnityEngine;

public static class FireStartChanceCalculator
{
    public static float Calculate(FireStartingConfig config, ItemData igniter, ItemData tinder, ItemData fuel, ItemData accelerant)
    {
        if (config == null)
        {
            return 0f;
        }

        if (accelerant != null)
        {
            return 100f;
        }

        float chance = config.BaseChance;

        chance += GetStartChanceBonus(igniter);
        chance += GetStartChanceBonus(tinder);
        chance += GetStartChanceBonus(fuel);

        return Mathf.Clamp(chance, 0f, 100f);
    }

    private static float GetStartChanceBonus(ItemData item)
    {
        return item != null ? item.StartChanceBonus : 0f;
    }
}