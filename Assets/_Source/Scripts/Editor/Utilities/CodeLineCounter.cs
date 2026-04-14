using System.IO;
using UnityEditor;
using UnityEngine;

public static class CodeLineCounter
{
    [MenuItem("Tools/Count Code Lines")]
    public static void CountLinesInFolder()
    {
        string targetFolder = EditorUtility.OpenFolderPanel("Select Folder to Count Code Lines", "Assets", "");

        if (string.IsNullOrEmpty(targetFolder))
        {
            Debug.LogWarning("Folder selection cancelled.");
            return;
        }

        string[] extensions = { ".cs", ".shader", ".cginc" };

        int totalLines = 0;
        int totalFiles = 0;

        foreach (string file in Directory.GetFiles(targetFolder, "*.*", SearchOption.AllDirectories))
        {
            string ext = Path.GetExtension(file);

            if (System.Array.Exists(extensions, e => e.Equals(ext, System.StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    int lineCount = File.ReadAllLines(file).Length;
                    totalLines += lineCount;
                    totalFiles++;
                }
                catch (IOException ex)
                {
                    Debug.LogWarning($"Could not read file: {file}\n{ex.Message}");
                }
            }
        }

        string relativePath = FilePathToProjectRelative(targetFolder);

        Debug.Log(
            $"<b>Code stats for:</b> {relativePath}\n" +
            $"<b>Files counted:</b> {totalFiles}\n" +
            $"<b>Total lines:</b> {totalLines}\n" +
            $"<b>Average lines per file:</b> {(totalFiles > 0 ? (totalLines / totalFiles) : 0)}"
        );
    }

    private static string FilePathToProjectRelative(string fullPath)
    {
        string projectPath = Application.dataPath;
        
        if (fullPath.StartsWith(projectPath))
        {
            return "Assets" + fullPath.Substring(projectPath.Length);
        }

        return fullPath;
    }
}