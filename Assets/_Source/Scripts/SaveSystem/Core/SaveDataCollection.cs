using System;
using System.Collections.Generic;

public static class SaveDataCollection
{
    public static T FindBySaveId<T>(IList<T> states, string saveId, Func<T, string> saveIdSelector)
        where T : class
    {
        if (states == null || string.IsNullOrWhiteSpace(saveId) || saveIdSelector == null)
        {
            return null;
        }

        for (int i = 0; i < states.Count; i++)
        {
            T state = states[i];

            if (state != null && saveIdSelector(state) == saveId)
            {
                return state;
            }
        }

        return null;
    }

    public static void RemoveBySaveId<T>(IList<T> states, string saveId, Func<T, string> saveIdSelector)
        where T : class
    {
        if (states == null || string.IsNullOrWhiteSpace(saveId) || saveIdSelector == null)
        {
            return;
        }

        for (int i = states.Count - 1; i >= 0; i--)
        {
            T state = states[i];

            if (state != null && saveIdSelector(state) == saveId)
            {
                states.RemoveAt(i);
            }
        }
    }
}
