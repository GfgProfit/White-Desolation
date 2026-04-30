using UnityEngine;

public sealed class JsonGameSaveSerializer : IGameSaveSerializer
{
    public string Serialize(GameSaveData saveData)
    {
        return JsonUtility.ToJson(saveData, true);
    }

    public GameSaveData Deserialize(string json)
    {
        return JsonUtility.FromJson<GameSaveData>(json);
    }
}
