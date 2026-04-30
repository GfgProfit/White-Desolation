using System;
using System.IO;
using UnityEngine;

public sealed class JsonSaveFileService
{
    private const string SaveDirectoryName = "Saves";
    private const string SaveExtension = ".json";
    private const string DefaultSlotName = "default";

    private readonly string _rootDirectory;

    public JsonSaveFileService(string rootDirectory = null)
    {
        _rootDirectory = string.IsNullOrWhiteSpace(rootDirectory) ? Application.persistentDataPath : rootDirectory;
    }

    public string GetSavePath(string slotName)
    {
        string safeSlotName = GetSafeSlotName(slotName);

        string directory = Path.Combine(_rootDirectory, SaveDirectoryName);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return Path.Combine(directory, safeSlotName + SaveExtension);
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
            string json = JsonUtility.ToJson(saveData, true);

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
            saveData = JsonUtility.FromJson<GameSaveData>(json);

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

    private static string GetSafeSlotName(string slotName)
    {
        if (string.IsNullOrWhiteSpace(slotName))
        {
            return DefaultSlotName;
        }

        string trimmedSlotName = slotName.Trim();
        char[] invalidChars = Path.GetInvalidFileNameChars();
        char[] safeChars = new char[trimmedSlotName.Length];
        int safeLength = 0;

        for (int i = 0; i < trimmedSlotName.Length; i++)
        {
            char character = trimmedSlotName[i];

            if (IsInvalidFileNameCharacter(character, invalidChars))
            {
                safeChars[safeLength] = '_';
            }
            else
            {
                safeChars[safeLength] = character;
            }

            safeLength++;
        }

        string safeSlotName = new(safeChars, 0, safeLength);

        return string.IsNullOrWhiteSpace(safeSlotName) ? DefaultSlotName : safeSlotName;
    }

    private static bool IsInvalidFileNameCharacter(char character, char[] invalidChars)
    {
        if (character == Path.DirectorySeparatorChar || character == Path.AltDirectorySeparatorChar)
        {
            return true;
        }

        for (int i = 0; i < invalidChars.Length; i++)
        {
            if (character == invalidChars[i])
            {
                return true;
            }
        }

        return false;
    }
}
