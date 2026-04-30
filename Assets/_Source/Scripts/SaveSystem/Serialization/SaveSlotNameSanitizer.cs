using System.IO;

public static class SaveSlotNameSanitizer
{
    public const string DefaultSlotName = "default";

    public static string Sanitize(string slotName)
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
            safeChars[safeLength] = IsInvalidFileNameCharacter(character, invalidChars) ? '_' : character;
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
