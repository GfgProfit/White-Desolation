using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class MainMenuController : MonoBehaviour
{
    [Header("Server")]
    [SerializeField] private string _serverBaseUrl = GameServerSettings.DefaultBaseUrl;
    [SerializeField] private string _slotName = GameServerSettings.DefaultSaveSlotName;

    [Header("Scenes")]
    [SerializeField] private string _authSceneName = "AuthMenuScene";
    [SerializeField] private string _gameplaySceneName = "TestScene";

    [Header("UI")]
    [SerializeField] private Button _playButton;
    [SerializeField] private TMP_Text _playButtonText;
    [SerializeField] private Button _exitButton;
    [SerializeField] private TMP_Text _playerNameText;

    private bool _isBusy;

    private void Awake()
    {
        CursorLockService.ForceUnlock();

        if (_playButton != null)
        {
            _playButton.onClick.AddListener(StartGame);
        }

        if (_exitButton != null)
        {
            _exitButton.onClick.AddListener(ExitGame);
        }
    }

    private void Start()
    {
        if (!PlayerSession.HasAuthenticatedUser)
        {
            SceneManager.LoadScene(_authSceneName);
            return;
        }

        RefreshPlayerName();
        RefreshPlayButton();
        RefreshSaveState();
    }

    private void OnDestroy()
    {
        if (_playButton != null)
        {
            _playButton.onClick.RemoveListener(StartGame);
        }

        if (_exitButton != null)
        {
            _exitButton.onClick.RemoveListener(ExitGame);
        }
    }

    public void StartGame()
    {
        if (_isBusy)
        {
            return;
        }

        SceneManager.LoadScene(_gameplaySceneName);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void RefreshSaveState()
    {
        _isBusy = true;
        SetPlayButtonInteractable(false);

        ServerSaveFileService saveFileService = new(_serverBaseUrl);
        bool hasSave = saveFileService.Exists(_slotName);
        PlayerSession.SetHasSave(hasSave);

        RefreshPlayButton();
        SetPlayButtonInteractable(true);
        _isBusy = false;
    }

    private void RefreshPlayerName()
    {
        if (_playerNameText != null)
        {
            _playerNameText.text = PlayerSession.Nickname;
        }
    }

    private void RefreshPlayButton()
    {
        if (_playButtonText != null)
        {
            _playButtonText.text = PlayerSession.HasSave ? "Продолжить" : "Новая игра";
        }
    }

    private void SetPlayButtonInteractable(bool interactable)
    {
        if (_playButton != null)
        {
            _playButton.interactable = interactable;
        }
    }
}