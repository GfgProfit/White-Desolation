using System.Collections.Generic;

public static class Utils
{
    public static string FormatNumber(long value, char separator)
    {
        if (value < 1000)
        {
            return value.ToString();
        }

        List<char> chars = new();

        var stringValue = value.ToString();
        var offset = 0;

        for (var i = stringValue.Length - 1; i >= 0; i--)
        {
            chars.Add(stringValue[i]);

            if ((chars.Count - offset) % 3 == 0)
            {
                chars.Add(separator);
                offset++;
            }
        }

        chars.Reverse();

        return string.Join("", chars.ToArray()).Trim(separator);
    }
}