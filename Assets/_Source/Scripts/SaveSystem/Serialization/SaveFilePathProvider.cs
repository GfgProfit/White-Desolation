using System.IO;
using UnityEngine;

public sealed class SaveFilePathProvider : ISavePathProvider
{
    private const string SaveDirectoryName = "Saves";
    private const string SaveExtension = ".json";

    private readonly string _rootDirectoryOverride;

    public SaveFilePathProvider(string rootDirectory = null)
    {
        _rootDirectoryOverride = rootDirectory;
    }

    public string GetSavePath(string slotName)
    {
        string rootDirectory = string.IsNullOrWhiteSpace(_rootDirectoryOverride) ? Application.persistentDataPath : _rootDirectoryOverride;
        string directory = Path.Combine(rootDirectory, SaveDirectoryName);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return Path.Combine(directory, SaveSlotNameSanitizer.Sanitize(slotName) + SaveExtension);
    }
}
