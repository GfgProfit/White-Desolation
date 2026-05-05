using System;
using UnityEngine;

[Serializable]
public sealed class SaveExistsResponse
{
    public bool exists;
    public string error;
}

public sealed class ServerSaveFileService : ISaveFileService
{
    private readonly string _baseUrl;
    private readonly IGameSaveSerializer _serializer;

    public ServerSaveFileService(string baseUrl = GameServerSettings.DefaultBaseUrl) : this(
        baseUrl,
        new JsonGameSaveSerializer())
    {
    }

    public ServerSaveFileService(string baseUrl, IGameSaveSerializer serializer)
    {
        _baseUrl = NormalizeBaseUrl(baseUrl);
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public string GetSavePath(string slotName)
    {
        if (!PlayerSession.HasAuthenticatedUser)
        {
            return string.Empty;
        }

        return BuildSaveUrl(slotName);
    }

    public void Save(string slotName, GameSaveData saveData)
    {
        if (saveData == null)
        {
            throw new ArgumentNullException(nameof(saveData));
        }

        if (!EnsureAuthenticated())
        {
            return;
        }

        string json = _serializer.Serialize(saveData);
        ServerHttpResponse response = ServerHttpClient.Send("PUT", BuildSaveUrl(slotName), json);

        if (!response.Success)
        {
            Debug.LogError($"[Save] Server save failed: {BuildError(response)}");
            return;
        }

        PlayerSession.SetHasSave(true);
        Debug.Log($"[Save] Saved to server slot '{slotName}' for user '{PlayerSession.Nickname}'.");
    }

    public bool TryLoad(string slotName, out GameSaveData saveData)
    {
        saveData = null;

        if (!EnsureAuthenticated())
        {
            return false;
        }

        ServerHttpResponse response = ServerHttpClient.Send("GET", BuildSaveUrl(slotName));

        if (response.StatusCode == 404)
        {
            PlayerSession.SetHasSave(false);
            return false;
        }

        if (!response.Success)
        {
            Debug.LogError($"[Save] Server load failed: {BuildError(response)}");
            return false;
        }

        try
        {
            saveData = _serializer.Deserialize(response.Body);
            bool loaded = saveData != null;
            PlayerSession.SetHasSave(loaded);
            return loaded;
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            return false;
        }
    }

    public bool Exists(string slotName)
    {
        if (!EnsureAuthenticated())
        {
            return false;
        }

        ServerHttpResponse response = ServerHttpClient.Send("GET", $"{BuildSaveUrl(slotName)}/exists");

        if (!response.Success)
        {
            Debug.LogError($"[Save] Server save check failed: {BuildError(response)}");
            return false;
        }

        SaveExistsResponse existsResponse = JsonUtility.FromJson<SaveExistsResponse>(response.Body);
        bool exists = existsResponse != null && existsResponse.exists;
        PlayerSession.SetHasSave(exists);
        return exists;
    }

    public void Delete(string slotName)
    {
        if (!EnsureAuthenticated())
        {
            return;
        }

        ServerHttpResponse response = ServerHttpClient.Send("DELETE", BuildSaveUrl(slotName));

        if (!response.Success)
        {
            Debug.LogError($"[Save] Server delete failed: {BuildError(response)}");
            return;
        }

        PlayerSession.SetHasSave(false);
    }

    private bool EnsureAuthenticated()
    {
        if (PlayerSession.HasAuthenticatedUser)
        {
            return true;
        }

        Debug.LogWarning("[Save] Cannot use server saves before player authorization.");
        return false;
    }

    private string BuildSaveUrl(string slotName)
    {
        string nickname = ServerHttpClient.EscapePathSegment(PlayerSession.Nickname);
        string slot = ServerHttpClient.EscapePathSegment(string.IsNullOrWhiteSpace(slotName) ? GameServerSettings.DefaultSaveSlotName : slotName);
        return $"{_baseUrl}/api/users/{nickname}/saves/{slot}";
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return GameServerSettings.DefaultBaseUrl;
        }

        return baseUrl.Trim().TrimEnd('/');
    }

    private static string BuildError(ServerHttpResponse response)
    {
        if (response == null)
        {
            return "No response.";
        }

        if (!string.IsNullOrWhiteSpace(response.Body))
        {
            SaveExistsResponse errorResponse = JsonUtility.FromJson<SaveExistsResponse>(response.Body);

            if (errorResponse != null && !string.IsNullOrWhiteSpace(errorResponse.error))
            {
                return errorResponse.error;
            }
        }

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.Error;
        }

        return $"HTTP {response.StatusCode}";
    }
}
