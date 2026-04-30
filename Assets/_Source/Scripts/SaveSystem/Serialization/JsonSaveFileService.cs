using System;
using System.IO;
using UnityEngine;

public sealed class JsonSaveFileService : ISaveFileService
{
    private readonly ISavePathProvider _pathProvider;
    private readonly IGameSaveSerializer _serializer;

    public JsonSaveFileService(string rootDirectory = null) : this(
        new SaveFilePathProvider(rootDirectory),
        new JsonGameSaveSerializer())
    {
    }

    public JsonSaveFileService(ISavePathProvider pathProvider, IGameSaveSerializer serializer)
    {
        _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public string GetSavePath(string slotName)
    {
        return _pathProvider.GetSavePath(slotName);
    }

    public void Save(string slotName, GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        try
        {
            string path = GetSavePath(slotName);
            string json = _serializer.Serialize(saveData);

            File.WriteAllText(path, json);

            Debug.Log($"[Save] Saved to: {path}");
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    public bool TryLoad(string slotName, out GameSaveData saveData)
    {
        try
        {
            string path = GetSavePath(slotName);

            if (!File.Exists(path))
            {
                saveData = null;
                return false;
            }

            string json = File.ReadAllText(path);
            saveData = _serializer.Deserialize(json);

            return saveData != null;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            saveData = null;
            return false;
        }
    }

    public bool Exists(string slotName)
    {
        try
        {
            return File.Exists(GetSavePath(slotName));
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            return false;
        }
    }

    public void Delete(string slotName)
    {
        try
        {
            string path = GetSavePath(slotName);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }
}
