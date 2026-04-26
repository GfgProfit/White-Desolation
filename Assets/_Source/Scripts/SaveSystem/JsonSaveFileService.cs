using System;
using System.IO;
using UnityEngine;

public sealed class JsonSaveFileService
{
    private const string SaveDirectoryName = "Saves";
    private const string SaveExtension = ".json";

    public string GetSavePath(string slotName)
    {
        if (string.IsNullOrWhiteSpace(slotName))
        {
            slotName = "default";
        }

        string directory = Path.Combine(Application.persistentDataPath, SaveDirectoryName);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return Path.Combine(directory, slotName + SaveExtension);
    }

    public void Save(string slotName, GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        string path = GetSavePath(slotName);
        string json = JsonUtility.ToJson(saveData, true);

        File.WriteAllText(path, json);

        Debug.Log($"[Save] Saved to: {path}");
    }

    public bool TryLoad(string slotName, out GameSaveData saveData)
    {
        string path = GetSavePath(slotName);

        if (!File.Exists(path))
        {
            saveData = null;
            return false;
        }

        string json = File.ReadAllText(path);
        saveData = JsonUtility.FromJson<GameSaveData>(json);

        return saveData != null;
    }

    public bool Exists(string slotName)
    {
        return File.Exists(GetSavePath(slotName));
    }

    public void Delete(string slotName)
    {
        string path = GetSavePath(slotName);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}