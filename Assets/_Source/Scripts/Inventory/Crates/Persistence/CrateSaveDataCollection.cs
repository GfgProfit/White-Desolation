using System.Collections.Generic;

public static class CrateSaveDataCollection
{
    public static CrateSaveData FindBySaveId(List<CrateSaveData> crates, string saveId)
    {
        if (crates == null || string.IsNullOrWhiteSpace(saveId))
        {
            return null;
        }

        for (int i = 0; i < crates.Count; i++)
        {
            CrateSaveData crate = crates[i];

            if (crate != null && crate.SaveId == saveId)
            {
                return crate;
            }
        }

        return null;
    }

    public static void RemoveBySaveId(List<CrateSaveData> crates, string saveId)
    {
        if (crates == null || string.IsNullOrWhiteSpace(saveId))
        {
            return;
        }

        for (int i = crates.Count - 1; i >= 0; i--)
        {
            if (crates[i] != null && crates[i].SaveId == saveId)
            {
                crates.RemoveAt(i);
            }
        }
    }
}
