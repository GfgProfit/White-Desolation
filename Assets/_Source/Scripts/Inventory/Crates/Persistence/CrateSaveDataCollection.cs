using System.Collections.Generic;

public static class CrateSaveDataCollection
{
    public static CrateSaveData FindBySaveId(List<CrateSaveData> crates, string saveId)
    {
        return SaveDataCollection.FindBySaveId(crates, saveId, crate => crate.SaveId);
    }

    public static void RemoveBySaveId(List<CrateSaveData> crates, string saveId)
    {
        SaveDataCollection.RemoveBySaveId(crates, saveId, crate => crate.SaveId);
    }
}
