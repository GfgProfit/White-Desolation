using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class AuthSceneController : MonoBehaviour
{
    [Header("Server")]
    [SerializeField] private string _serverBaseUrl = GameServerSettings.DefaultBaseUrl;

    [Header("Scene")]
    [SerializeField] private string _mainMenuSceneName = "MainMenuScene";

    [Header("UI")]
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private Button _loginButton;
    [SerializeField] private TMP_Text _statusText;

    private bool _isBusy;

    private void Awake()
    {
        CursorLockService.ForceUnlock();

        if (_loginButton != null)
        {
            _loginButton.onClick.AddListener(Login);
        }
    }

    private void Start()
    {
        if (_nicknameInput != null)
        {
            _nicknameInput.Select();
        }

        SetStatus(string.Empty);
    }

    private void OnDestroy()
    {
        if (_loginButton != null)
        {
            _loginButton.onClick.RemoveListener(Login);
        }
    }

    public void Login()
    {
        if (_isBusy)
        {
            return;
        }

        string nickname = _nicknameInput != null ? _nicknameInput.text : string.Empty;

        if (string.IsNullOrWhiteSpace(nickname))
        {
            SetStatus("Введите ник.");
            return;
        }

        _isBusy = true;
        SetControlsInteractable(false);
        SetStatus("Подключение...");

        GameServerClient client = new(_serverBaseUrl);

        if (!client.TryAuthorize(nickname.Trim(), out PlayerAuthResponse response, out string error))
        {
            Fail(error);
            SetControlsInteractable(true);
            _isBusy = false;
            return;
        }

        if (!ApplyAuthResponse(response, out error))
        {
            Fail(error);
            SetControlsInteractable(true);
            _isBusy = false;
            return;
        }

        SceneManager.LoadScene(_mainMenuSceneName);
    }

    private static bool ApplyAuthResponse(PlayerAuthResponse response, out string error)
    {
        if (response == null || string.IsNullOrWhiteSpace(response.nickname))
        {
            error = "Сервер вернул некорректный ответ.";
            return false;
        }

        PlayerSession.SetAuthenticatedUser(response.nickname, response.existed, response.hasSave);
        error = null;
        return true;
    }

    private void Fail(string error)
    {
        SetStatus(string.IsNullOrWhiteSpace(error) ? "Ошибка авторизации." : error);
    }

    private void SetControlsInteractable(bool interactable)
    {
        if (_nicknameInput != null)
        {
            _nicknameInput.interactable = interactable;
        }

        if (_loginButton != null)
        {
            _loginButton.interactable = interactable;
        }
    }

    private void SetStatus(string value)
    {
        if (_statusText != null)
        {
            _statusText.text = value;
        }
    }
}