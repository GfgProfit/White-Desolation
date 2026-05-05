using System;
using UnityEngine;

[Serializable]
public sealed class PlayerAuthRequest
{
    public string nickname;
}

[Serializable]
public sealed class PlayerAuthResponse
{
    public string nickname;
    public bool existed;
    public bool hasSave;
    public string error;
}

public sealed class GameServerClient
{
    private readonly string _baseUrl;

    public GameServerClient(string baseUrl = GameServerSettings.DefaultBaseUrl)
    {
        _baseUrl = NormalizeBaseUrl(baseUrl);
    }

    public bool TryAuthorize(string nickname, out PlayerAuthResponse authResponse, out string error)
    {
        authResponse = null;
        error = null;

        if (string.IsNullOrWhiteSpace(nickname))
        {
            error = "Введите ник.";
            return false;
        }

        PlayerAuthRequest request = new()
        {
            nickname = nickname.Trim()
        };

        string url = $"{_baseUrl}/api/users";
        string json = JsonUtility.ToJson(request);
        ServerHttpResponse response = ServerHttpClient.Send("POST", url, json);

        if (!response.Success)
        {
            error = BuildError(response, "Не удалось авторизоваться.");
            return false;
        }

        authResponse = JsonUtility.FromJson<PlayerAuthResponse>(response.Body);

        if (authResponse == null || string.IsNullOrWhiteSpace(authResponse.nickname))
        {
            error = "Сервер вернул некорректный ответ.";
            return false;
        }

        return true;
    }

    private static string NormalizeBaseUrl(string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return GameServerSettings.DefaultBaseUrl;
        }

        return baseUrl.Trim().TrimEnd('/');
    }

    private static string BuildError(ServerHttpResponse response, string fallbackMessage)
    {
        if (response == null)
        {
            return fallbackMessage;
        }

        if (!string.IsNullOrWhiteSpace(response.Body))
        {
            PlayerAuthResponse errorResponse = JsonUtility.FromJson<PlayerAuthResponse>(response.Body);

            if (errorResponse != null && !string.IsNullOrWhiteSpace(errorResponse.error))
            {
                return errorResponse.error;
            }
        }

        if (!string.IsNullOrWhiteSpace(response.Error))
        {
            return response.Error;
        }

        return fallbackMessage;
    }
}
