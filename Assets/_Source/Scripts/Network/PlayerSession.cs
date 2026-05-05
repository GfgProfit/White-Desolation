using UnityEngine;

public static class PlayerSession
{
    private const string NicknamePlayerPrefsKey = "WhiteDesolation.PlayerNickname";

    public static string Nickname { get; private set; }
    public static bool UserExisted { get; private set; }
    public static bool HasSave { get; private set; }

    public static bool HasAuthenticatedUser => !string.IsNullOrWhiteSpace(Nickname);

    public static void SetAuthenticatedUser(string nickname, bool userExisted, bool hasSave)
    {
        Nickname = nickname;
        UserExisted = userExisted;
        HasSave = hasSave;

        PlayerPrefs.SetString(NicknamePlayerPrefsKey, Nickname);
        PlayerPrefs.Save();
    }

    public static bool TryRestoreLastUser()
    {
        string nickname = PlayerPrefs.GetString(NicknamePlayerPrefsKey, string.Empty);

        if (string.IsNullOrWhiteSpace(nickname))
        {
            return false;
        }

        Nickname = nickname;
        UserExisted = true;
        return true;
    }

    public static void SetHasSave(bool hasSave)
    {
        HasSave = hasSave;
    }

    public static void Clear()
    {
        Nickname = null;
        UserExisted = false;
        HasSave = false;
        PlayerPrefs.DeleteKey(NicknamePlayerPrefsKey);
        PlayerPrefs.Save();
    }
}
