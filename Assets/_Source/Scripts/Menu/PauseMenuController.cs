using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-10000)]
public sealed class PauseMenuController : MonoBehaviour
{
    private const string MainMenuSceneName = "MainMenuScene";

    [Header("Scene")]
    [SerializeField] private string _mainMenuSceneName = MainMenuSceneName;

    [Header("Links")]
    [SerializeField] private CrateUIController _crateUIController;
    [SerializeField] private InventoryUIController _inventoryUIController;
    [SerializeField] private FireUIController _fireUIController;

    [Header("UI")]
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _loadButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private TMP_Text _statusText;

    [Header("Pause Lock")]
    [SerializeField] private Behaviour[] _disableWhilePaused;
    [SerializeField] private GameObject[] _objectDisableWhilePaused;

    private PlayerControlLockSession _controlLockSession;
    private SaveManager _saveManager;
    private bool _isOpen;
    private float _previousTimeScale = 1f;

    private void Awake()
    {
        EnsureReferences();
        BindButtons();
        CloseImmediate();
    }

    private void OnDestroy()
    {
        UnbindButtons();
        Resume();
        _controlLockSession?.Release();
        CursorLockService.ReleaseOwner(this);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        if (_isOpen)
        {
            Resume();
            return;
        }

        if (TryCloseOpenGameplayWindow())
        {
            return;
        }

        Open();
    }

    public void Resume()
    {
        if (!_isOpen)
        {
            return;
        }

        _isOpen = false;
        Time.timeScale = Mathf.Approximately(_previousTimeScale, 0f) ? 1f : _previousTimeScale;
        SetRootActive(false);

        _controlLockSession?.Unlock();
        CursorLockService.ReleaseCursor(this);
    }

    public void LoadGame()
    {
        EnsureReferences();

        if (_saveManager == null)
        {
            SetStatus("SaveManager не найден.");
            return;
        }

        _saveManager.Load();
        Resume();
    }

    public void SaveGame()
    {
        EnsureReferences();

        if (_saveManager == null)
        {
            SetStatus("SaveManager не найден.");
            return;
        }

        _saveManager.Save();
        SetStatus("Игра сохранена.");
    }

    public void ExitToMainMenu()
    {
        _isOpen = false;
        Time.timeScale = 1f;
        SetRootActive(false);

        _controlLockSession?.Unlock();
        CursorLockService.ReleaseCursor(this);
        CursorLockService.ForceUnlock();

        SceneManager.LoadScene(_mainMenuSceneName);
    }

    private void Open()
    {
        if (_root == null)
        {
            Debug.LogWarning("[PauseMenu] Root is not assigned.");
            return;
        }

        EnsureReferences();

        _previousTimeScale = Time.timeScale;
        _isOpen = true;
        Time.timeScale = 0f;
        SetStatus(string.Empty);
        SetRootActive(true);

        CursorLockService.ShowCursor(this);
        _controlLockSession?.Lock();
    }

    private void CloseImmediate()
    {
        _isOpen = false;
        SetRootActive(false);
    }

    private void EnsureReferences()
    {
        _saveManager = FindFirstObjectByType<SaveManager>();

        if (_disableWhilePaused == null || _disableWhilePaused.Length == 0)
        {
            _disableWhilePaused = FindDefaultBehavioursToDisable();
            _controlLockSession = null;
        }

        _controlLockSession ??= PlayerControlLockService.CreateSession(this, _disableWhilePaused, _objectDisableWhilePaused);
    }

    private static Behaviour[] FindDefaultBehavioursToDisable()
    {
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        InteractController interactController = FindFirstObjectByType<InteractController>();

        if (playerController != null && interactController != null)
        {
            return new Behaviour[] { playerController, interactController };
        }

        if (playerController != null)
        {
            return new Behaviour[] { playerController };
        }

        if (interactController != null)
        {
            return new Behaviour[] { interactController };
        }

        return System.Array.Empty<Behaviour>();
    }

    private bool TryCloseOpenGameplayWindow()
    {
        bool closed = false;

        closed |= _crateUIController != null && _crateUIController.TryCloseOpenWindow();
        closed |= _inventoryUIController != null && _inventoryUIController.TryCloseOpenWindow();
        closed |= _fireUIController != null && _fireUIController.TryCloseOpenWindow();

        return closed;
    }

    private void BindButtons()
    {
        if (_resumeButton != null)
        {
            _resumeButton.onClick.AddListener(Resume);
        }

        if (_loadButton != null)
        {
            _loadButton.onClick.AddListener(LoadGame);
        }

        if (_saveButton != null)
        {
            _saveButton.onClick.AddListener(SaveGame);
        }

        if (_mainMenuButton != null)
        {
            _mainMenuButton.onClick.AddListener(ExitToMainMenu);
        }
    }

    private void UnbindButtons()
    {
        if (_resumeButton != null)
        {
            _resumeButton.onClick.RemoveListener(Resume);
        }

        if (_loadButton != null)
        {
            _loadButton.onClick.RemoveListener(LoadGame);
        }

        if (_saveButton != null)
        {
            _saveButton.onClick.RemoveListener(SaveGame);
        }

        if (_mainMenuButton != null)
        {
            _mainMenuButton.onClick.RemoveListener(ExitToMainMenu);
        }
    }

    private void SetRootActive(bool active)
    {
        if (_root != null)
        {
            _root.SetActive(active);
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
