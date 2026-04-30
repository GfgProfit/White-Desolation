public interface IGameSaveSerializer
{
    string Serialize(GameSaveData saveData);
    GameSaveData Deserialize(string json);
}
