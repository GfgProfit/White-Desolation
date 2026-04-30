using System.IO;
using UnityEngine;

public sealed class SaveFilePathProvider : ISavePathProvider
{
    private const string SaveDirectoryName = "Saves";
    private const string SaveExtension = ".json";

    private readonly string _rootDirectory;

    public SaveFilePathProvider(string rootDirectory = null)
    {
        _rootDirectory = string.IsNullOrWhiteSpace(rootDirectory) ? Application.persistentDataPath : rootDirectory;
    }

    public string GetSavePath(string slotName)
    {
        string directory = Path.Combine(_rootDirectory, SaveDirectoryName);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return Path.Combine(directory, SaveSlotNameSanitizer.Sanitize(slotName) + SaveExtension);
    }
}
