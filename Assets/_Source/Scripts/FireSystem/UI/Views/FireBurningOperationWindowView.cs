using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class FireBurningOperationWindowView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Button _addFuelTabButton;
    [SerializeField] private Button _cookingTabButton;
    [SerializeField] private Button _waterTabButton;
    [SerializeField] private TMP_Text _burningTimeText;
    [SerializeField] private Transform _listRoot;
    [SerializeField] private FireOperationListItemView _listItemPrefab;
    [SerializeField] private Button _actionButton;
    [SerializeField] private TMP_Text _actionButtonText;
    [SerializeField] private Button _closeButton;

    public GameObject Root => _root;
    public Button AddFuelTabButton => _addFuelTabButton;
    public Button CookingTabButton => _cookingTabButton;
    public Button WaterTabButton => _waterTabButton;
    public TMP_Text BurningTimeText => _burningTimeText;
    public Transform ListRoot => _listRoot;
    public FireOperationListItemView ListItemPrefab => _listItemPrefab;
    public Button ActionButton => _actionButton;
    public TMP_Text ActionButtonText => _actionButtonText;
    public Button CloseButton => _closeButton;
}
