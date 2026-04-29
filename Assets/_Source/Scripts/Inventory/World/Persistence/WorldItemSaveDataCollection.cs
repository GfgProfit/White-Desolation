using System.Collections.Generic;

public static class WorldItemSaveDataCollection
{
    public static WorldItemSaveData FindBySaveId(List<WorldItemSaveData> states, string saveId)
    {
        if (states == null || string.IsNullOrWhiteSpace(saveId))
        {
            return null;
        }

        for (int i = 0; i < states.Count; i++)
        {
            WorldItemSaveData state = states[i];

            if (state != null && state.SaveId == saveId)
            {
                return state;
            }
        }

        return null;
    }

    public static void RemoveBySaveId(List<WorldItemSaveData> states, string saveId)
    {
        if (states == null || string.IsNullOrWhiteSpace(saveId))
        {
            return;
        }

        for (int i = states.Count - 1; i >= 0; i--)
        {
            if (states[i] != null && states[i].SaveId == saveId)
            {
                states.RemoveAt(i);
            }
        }
    }
}