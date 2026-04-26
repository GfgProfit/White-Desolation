using UnityEngine;

public static class FireIgniterConsumptionPolicy
{
    public static bool TryGetDurabilityCost(ItemData igniter, out float durabilityCost)
    {
        durabilityCost = 0f;

        if (igniter == null)
        {
            return false;
        }

        if (!igniter.UsesDurability || igniter.IsUnbreakable)
        {
            return false;
        }

        bool shouldConsumeDurability = igniter.FireIgniterConsumeMode switch
        {
            FireIgniterConsumeMode.ConsumeDurability => true,
            FireIgniterConsumeMode.ConsumeItem => false,
            _ => true
        };

        if (!shouldConsumeDurability)
        {
            return false;
        }

        durabilityCost = Mathf.Max(0f, igniter.MaxDurability * igniter.FireIgniterDurabilityCost01);
        return durabilityCost > 0f;
    }
}