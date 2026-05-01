using System.Collections.Generic;

public static class FireSourceSaveDataCollection
{
    public static FireSourceSaveData FindBySaveId(List<FireSourceSaveData> states, string saveId)
    {
        return SaveDataCollection.FindBySaveId(states, saveId, state => state.SaveId);
    }

    public static void RemoveBySaveId(List<FireSourceSaveData> states, string saveId)
    {
        SaveDataCollection.RemoveBySaveId(states, saveId, state => state.SaveId);
    }
}
