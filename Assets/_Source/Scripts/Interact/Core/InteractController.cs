using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class InteractController : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField, Min(0.1f)] private float _interactRange = 3.0f;

    [Header("Hover UI")]
    [SerializeField] private GameObject _hoverRoot;
    [SerializeField] private CanvasGroup _hoverCanvasGroup;
    [SerializeField] private TMP_Text _hoverNameText;

    [Header("Hover UI Extra")]
    [SerializeField] private Image _lineImage;

    [SerializeField] private GameObject _timeHolder;
    [SerializeField] private TMP_Text _timeText;

    [SerializeField] private GameObject _temperatureHolder;
    [SerializeField] private TMP_Text _temperatureText;

    [SerializeField] private GameObject _infoHolder;
    [SerializeField] private TMP_Text _infoText;

    [Header("Hover Fade")]
    [SerializeField, Min(0f)] private float _hoverFadeDuration = 0.15f;

    [Header("Inspect UI")]
    [SerializeField] private GameObject _inspectRoot;
    [SerializeField] private Image _inspectIcon;
    [SerializeField] private Image _durabilityIcon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _durabilityText;
    [SerializeField] private TMP_Text _weightText;

    [Header("Optional")]
    [SerializeField] private Behaviour[] _disableWhileInspectOpen;
    [SerializeField] private GameObject[] _objectDisableWhileInspectOpen;

    [Inject] private IPlayerInput _playerInput = null;

    private InteractionTargetService _targetService;
    private InteractionHoverInfoQuery _hoverInfoQuery;
    private InteractionInputReader _inputReader;
    private InteractionInputService _inputService;
    private InteractionInspectActionService _inspectActionService;
    private GenericInteractionExecutionService _executionService;
    private InteractionHoverPresenter _hoverPresenter;
    private InteractionInspectPresenter _inspectPresenter;
    private InteractionInspectSessionController _inspectSession;
    private InteractionTarget _currentTarget;
    private InteractionInputState _currentInputState;
    private bool IsInspectOpen => _inspectSession != null && _inspectSession.IsOpen;
}
