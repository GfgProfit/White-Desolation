using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class InventoryUIController
{
    private const string CraftProgressText = "создание";
    private const float SelectedInventoryWindowButtonAlpha = 1f;
    private const float UnselectedInventoryWindowButtonAlpha = 0.2f;

    private enum InventoryWindowTab
    {
        Inventory,
        Craft
    }

    [Header("Inventory / Craft Tabs")]
    [SerializeField] private GameObject _inventoryWindow;
    [SerializeField] private GameObject _craftWindow;
    [SerializeField] private Button _inventoryTabButton;
    [SerializeField] private CanvasGroup _inventoryTabCanvasGroup;
    [SerializeField] private Button _craftTabButton;
    [SerializeField] private CanvasGroup _craftTabCanvasGroup;

    [Header("Crafting Data")]
    [SerializeField] private CraftRecipe[] _craftRecipeAssets;
    [SerializeField, Min(0.01f)] private float _craftDurationSeconds = 10f;

    [Header("Crafting List")]
    [SerializeField] private Transform _craftListRoot;
    [SerializeField] private GameObject _craftListItemPrefab;

    [Header("Crafting Details")]
    [SerializeField] private Image _craftItemIcon;
    [SerializeField] private TMP_Text _craftItemNameText;
    [SerializeField] private TMP_Text _craftItemDescriptionText;
    [SerializeField] private Transform _craftRequirementsRoot;
    [SerializeField] private GameObject _craftRequireItemPrefab;
    [SerializeField] private GameObject _craftDetailsRightWindow;
    [SerializeField] private TMP_Text _craftTimeText;
    [SerializeField] private Button _craftActionButton;

    [Header("Crafting Tool")]
    [SerializeField] private GameObject _craftToolInfoRoot;
    [SerializeField] private GameObject _craftToolItemViewRoot;
    [SerializeField] private GameObject _craftNoToolTextRoot;
    [SerializeField] private Image _craftToolIconImage;
    [SerializeField] private TMP_Text _craftToolNameText;
    [SerializeField] private TMP_Text _craftToolDurabilityText;
    [SerializeField] private Image _craftToolDurabilityIcon;
    [SerializeField] private GameObject _craftToolStatsRoot;
    [SerializeField] private Button _craftToolLeftButton;
    [SerializeField] private Button _craftToolRightButton;

    [Inject(true)] private IGameTimeAdvancer _craftGameTimeAdvancer = null;
    [Inject(true)] private IGameTimeRunController _craftGameTimeRunController = null;

    private readonly List<CraftRecipe> _craftRecipes = new();
    private readonly List<CraftListItemView> _spawnedCraftListItems = new();
    private readonly List<CraftRequireItemView> _spawnedCraftRequirementItems = new();
    private readonly List<CraftToolCandidate> _craftToolCandidates = new();
    private readonly List<CraftToolCandidate> _craftListToolCandidatesScratch = new();
    private CraftingService _craftingService;
    private InventoryWindowTab _activeWindowTab = InventoryWindowTab.Inventory;
    private CraftRecipe _selectedCraftRecipe;
    private string _selectedCraftRecipeId;
    private CraftRecipe[] _resourceCraftRecipes;
    private int _selectedCraftToolIndex = -1;
    private Coroutine _craftRoutine;
    private bool _craftingAdvancingTime;
    private bool _restoreCraftGameTimeRunning;
    private bool _previousCraftGameTimeRunning;

    private bool IsCrafting => _craftRoutine != null;

    private void InitializeCrafting()
    {
        ResolveCraftingReferences();

        _craftingService = new CraftingService(_inventoryController);

        if (_inventoryTabButton != null)
        {
            _inventoryTabButton.onClick.AddListener(HandleInventoryTabClicked);
        }

        if (_craftTabButton != null)
        {
            _craftTabButton.onClick.AddListener(HandleCraftTabClicked);
        }

        if (_craftActionButton != null)
        {
            _craftActionButton.onClick.AddListener(HandleCraftActionClicked);
        }

        if (_craftToolLeftButton != null)
        {
            _craftToolLeftButton.onClick.AddListener(HandlePreviousCraftToolClicked);
        }

        if (_craftToolRightButton != null)
        {
            _craftToolRightButton.onClick.AddListener(HandleNextCraftToolClicked);
        }

        ShowInventoryWindowTab(false);
    }

    private void CleanupCrafting()
    {
        if (_inventoryTabButton != null)
        {
            _inventoryTabButton.onClick.RemoveListener(HandleInventoryTabClicked);
        }

        if (_craftTabButton != null)
        {
            _craftTabButton.onClick.RemoveListener(HandleCraftTabClicked);
        }

        if (_craftActionButton != null)
        {
            _craftActionButton.onClick.RemoveListener(HandleCraftActionClicked);
        }

        if (_craftToolLeftButton != null)
        {
            _craftToolLeftButton.onClick.RemoveListener(HandlePreviousCraftToolClicked);
        }

        if (_craftToolRightButton != null)
        {
            _craftToolRightButton.onClick.RemoveListener(HandleNextCraftToolClicked);
        }

        ClearCraftList();
        ClearCraftRequirements();
    }

    private void StopCraftRoutineAndResetProgress()
    {
        if (_craftRoutine != null)
        {
            StopCoroutine(_craftRoutine);
            _craftRoutine = null;
        }

        EndCraftingTimeAdvance();

        _useProgressModal?.HideAndReset();
    }

    private void HandleInventoryTabClicked()
    {
        if (IsCrafting || _useRoutineState.IsUsingItem)
        {
            return;
        }

        ShowInventoryWindowTab(true);
    }

    private void HandleCraftTabClicked()
    {
        if (IsCrafting || _useRoutineState.IsUsingItem)
        {
            return;
        }

        ShowCraftWindowTab();
    }

    private void ShowInventoryWindowTab(bool refresh)
    {
        _activeWindowTab = InventoryWindowTab.Inventory;

        SetActiveSafe(_inventoryWindow, true);
        SetActiveSafe(_craftWindow, false);

        RefreshInventoryWindowTabVisuals();

        if (refresh)
        {
            RefreshView();
        }
    }

    private void ShowCraftWindowTab()
    {
        _activeWindowTab = InventoryWindowTab.Craft;

        SetActiveSafe(_inventoryWindow, false);
        SetActiveSafe(_craftWindow, true);

        RefreshInventoryWindowTabVisuals();
        RefreshCraftView();
    }

    private void RefreshInventoryWindowTabVisuals()
    {
        SetCanvasGroupAlpha(_inventoryTabCanvasGroup, _activeWindowTab == InventoryWindowTab.Inventory ? SelectedInventoryWindowButtonAlpha : UnselectedInventoryWindowButtonAlpha);
        SetCanvasGroupAlpha(_craftTabCanvasGroup, _activeWindowTab == InventoryWindowTab.Craft ? SelectedInventoryWindowButtonAlpha : UnselectedInventoryWindowButtonAlpha);
    }

    private void RefreshCraftView()
    {
        if (_inventoryController == null)
        {
            return;
        }

        RebuildCraftRecipes();
        ValidateSelectedCraftRecipe();
        RefreshCraftToolCandidates();
        RebuildCraftList();
        RefreshCraftDetails();
    }

    private void RebuildCraftRecipes()
    {
        _craftRecipes.Clear();

        AddCraftRecipes(_craftRecipeAssets);
        AddCraftRecipes(GetResourceCraftRecipes());

        _craftRecipes.Sort(CompareCraftRecipesByName);
    }

    private IReadOnlyList<CraftRecipe> GetResourceCraftRecipes()
    {
        _resourceCraftRecipes ??= Resources.LoadAll<CraftRecipe>("Craft Recipes");

        return _resourceCraftRecipes;
    }

    private void AddCraftRecipes(IReadOnlyList<CraftRecipe> recipes)
    {
        if (recipes == null)
        {
            return;
        }

        for (int i = 0; i < recipes.Count; i++)
        {
            CraftRecipe recipe = recipes[i];

            if (recipe == null || !recipe.IsValid || _craftRecipes.Contains(recipe))
            {
                continue;
            }

            _craftRecipes.Add(recipe);
        }
    }

    private static int CompareCraftRecipesByName(CraftRecipe left, CraftRecipe right)
    {
        string leftName = left != null && left.ResultItem != null ? left.ResultItem.DisplayName : string.Empty;
        string rightName = right != null && right.ResultItem != null ? right.ResultItem.DisplayName : string.Empty;

        int nameComparison = string.Compare(leftName, rightName, System.StringComparison.CurrentCulture);

        if (nameComparison != 0)
        {
            return nameComparison;
        }

        string leftId = left != null ? left.Id : string.Empty;
        string rightId = right != null ? right.Id : string.Empty;

        return string.Compare(leftId, rightId, System.StringComparison.Ordinal);
    }

    private void ValidateSelectedCraftRecipe()
    {
        if (_selectedCraftRecipe != null && _craftRecipes.Contains(_selectedCraftRecipe))
        {
            return;
        }

        _selectedCraftRecipe = FindCraftRecipeById(_selectedCraftRecipeId);

        if (_selectedCraftRecipe == null)
        {
            _selectedCraftRecipe = _craftRecipes.Count > 0 ? _craftRecipes[0] : null;
        }

        _selectedCraftRecipeId = _selectedCraftRecipe != null ? _selectedCraftRecipe.Id : null;
        _selectedCraftToolIndex = -1;
    }

    private CraftRecipe FindCraftRecipeById(string recipeId)
    {
        if (string.IsNullOrWhiteSpace(recipeId))
        {
            return null;
        }

        for (int i = 0; i < _craftRecipes.Count; i++)
        {
            CraftRecipe recipe = _craftRecipes[i];

            if (recipe != null && recipe.Id == recipeId)
            {
                return recipe;
            }
        }

        return null;
    }

    private void RebuildCraftList()
    {
        ClearCraftList();

        if (_craftListRoot == null || _craftListItemPrefab == null)
        {
            return;
        }

        for (int i = 0; i < _craftRecipes.Count; i++)
        {
            CraftRecipe recipe = _craftRecipes[i];
            GameObject instance = Instantiate(_craftListItemPrefab, _craftListRoot);
            CraftListItemView view = instance.GetComponent<CraftListItemView>();

            if (view == null)
            {
                view = instance.AddComponent<CraftListItemView>();
            }

            bool canCraft = CanCraftListItem(recipe);
            view.Bind(recipe, canCraft, HandleCraftRecipeSelected);

            _spawnedCraftListItems.Add(view);
        }
    }

    private void ClearCraftList()
    {
        for (int i = 0; i < _spawnedCraftListItems.Count; i++)
        {
            if (_spawnedCraftListItems[i] != null)
            {
                Destroy(_spawnedCraftListItems[i].gameObject);
            }
        }

        _spawnedCraftListItems.Clear();
    }

    private void HandleCraftRecipeSelected(CraftRecipe recipe)
    {
        if (IsCrafting || _useRoutineState.IsUsingItem || recipe == null)
        {
            return;
        }

        if (_selectedCraftRecipe == recipe)
        {
            RefreshCraftView();
            return;
        }

        _selectedCraftRecipe = recipe;
        _selectedCraftRecipeId = recipe.Id;
        _selectedCraftToolIndex = -1;

        RefreshCraftView();
    }

    private void RefreshCraftDetails()
    {
        CraftRecipe recipe = _selectedCraftRecipe;
        ItemData resultItem = recipe != null ? recipe.ResultItem : null;
        bool hasSelection = recipe != null && resultItem != null;

        if (_craftDetailsRightWindow != null)
        {
            _craftDetailsRightWindow.SetActive(hasSelection);
        }

        if (_craftItemIcon != null)
        {
            Sprite icon = resultItem != null ? resultItem.Icon : null;
            _craftItemIcon.enabled = icon != null;
            _craftItemIcon.sprite = icon;
        }

        if (_craftItemNameText != null)
        {
            _craftItemNameText.text = resultItem != null ? resultItem.DisplayName : string.Empty;
        }

        if (_craftItemDescriptionText != null)
        {
            _craftItemDescriptionText.text = resultItem != null ? resultItem.Description : string.Empty;
        }

        RebuildCraftRequirements();
        RefreshCraftToolDetails();

        if (_craftTimeText != null)
        {
            _craftTimeText.text = recipe != null ? FormatCraftTime(recipe.GameMinutes) : string.Empty;
        }

        if (_craftActionButton != null)
        {
            _craftActionButton.interactable = hasSelection && !IsCrafting && !_useRoutineState.IsUsingItem && _craftingService != null && _craftingService.CanCraft(recipe, GetSelectedCraftTool());
        }
    }

    private void RebuildCraftRequirements()
    {
        ClearCraftRequirements();

        if (_craftRequirementsRoot == null || _craftRequireItemPrefab == null || _selectedCraftRecipe == null)
        {
            return;
        }

        IReadOnlyList<CraftRequirement> requirements = _selectedCraftRecipe.Requirements;

        if (requirements == null)
        {
            return;
        }

        for (int i = 0; i < requirements.Count; i++)
        {
            CraftRequirement requirement = requirements[i];

            if (requirement == null || !requirement.IsValid)
            {
                continue;
            }

            if (CraftingInventoryQuery.HasPreviousRequirementForItem(requirements, requirement.Item, i))
            {
                continue;
            }

            GameObject instance = Instantiate(_craftRequireItemPrefab, _craftRequirementsRoot);
            CraftRequireItemView view = instance.GetComponent<CraftRequireItemView>();

            if (view == null)
            {
                view = instance.AddComponent<CraftRequireItemView>();
            }

            int ownedCount = CraftingInventoryQuery.GetOwnedCount(_inventoryController, requirement.Item);
            int requiredCount = CraftingInventoryQuery.GetRequiredCount(requirements, requirement.Item);
            view.Bind(requirement.Item, ownedCount, requiredCount);

            _spawnedCraftRequirementItems.Add(view);
        }
    }

    private void ClearCraftRequirements()
    {
        for (int i = 0; i < _spawnedCraftRequirementItems.Count; i++)
        {
            if (_spawnedCraftRequirementItems[i] != null)
            {
                Destroy(_spawnedCraftRequirementItems[i].gameObject);
            }
        }

        _spawnedCraftRequirementItems.Clear();
    }

    private void RefreshCraftToolCandidates()
    {
        int previousSlotIndex = GetSelectedCraftTool()?.SlotIndex ?? -1;

        CraftingInventoryQuery.BuildToolCandidates(_inventoryController, _selectedCraftRecipe, _craftToolCandidates);

        if (_craftToolCandidates.Count == 0)
        {
            _selectedCraftToolIndex = -1;
            return;
        }

        if (previousSlotIndex >= 0)
        {
            for (int i = 0; i < _craftToolCandidates.Count; i++)
            {
                if (_craftToolCandidates[i].SlotIndex == previousSlotIndex)
                {
                    _selectedCraftToolIndex = i;
                    return;
                }
            }
        }

        if (_selectedCraftToolIndex < 0 || _selectedCraftToolIndex >= _craftToolCandidates.Count)
        {
            _selectedCraftToolIndex = 0;
        }
    }

    private void RefreshCraftToolDetails()
    {
        bool hasToolRequirement = CraftingInventoryQuery.HasToolRequirement(_selectedCraftRecipe);

        SetActiveSafe(_craftToolInfoRoot, hasToolRequirement);

        if (!hasToolRequirement)
        {
            return;
        }

        CraftToolCandidate? selectedTool = GetSelectedCraftTool();
        bool hasTool = selectedTool.HasValue;

        SetActiveSafe(_craftNoToolTextRoot, !hasTool);
        SetActiveSafe(_craftToolItemViewRoot, hasTool);
        SetActiveSafe(_craftToolStatsRoot, hasTool);

        bool canSwitchTool = _craftToolCandidates.Count > 1 && !IsCrafting;

        if (_craftToolLeftButton != null)
        {
            _craftToolLeftButton.interactable = canSwitchTool;
        }

        if (_craftToolRightButton != null)
        {
            _craftToolRightButton.interactable = canSwitchTool;
        }

        if (!hasTool)
        {
            ClearCraftToolDisplay();
            return;
        }

        CraftToolCandidate tool = selectedTool.Value;
        InventorySlot slot = tool.Slot;

        if (_craftToolIconImage != null)
        {
            Sprite icon = slot != null && slot.Item != null ? slot.Item.Icon : null;
            _craftToolIconImage.enabled = icon != null;
            _craftToolIconImage.sprite = icon;
        }

        if (_craftToolNameText != null)
        {
            _craftToolNameText.text = slot != null && slot.Item != null ? slot.Item.DisplayName : string.Empty;
        }

        if (_craftToolDurabilityText != null)
        {
            _craftToolDurabilityText.text = InventoryDisplayFormatter.FormatDurabilityShort(slot);
        }

        if (slot != null)
        {
            Utils.SetDurabilityColor01(slot.Durability01, _craftToolDurabilityText, _craftToolDurabilityIcon);
        }
    }

    private void ClearCraftToolDisplay()
    {
        if (_craftToolIconImage != null)
        {
            _craftToolIconImage.enabled = false;
            _craftToolIconImage.sprite = null;
        }

        if (_craftToolNameText != null)
        {
            _craftToolNameText.text = string.Empty;
        }

        if (_craftToolDurabilityText != null)
        {
            _craftToolDurabilityText.text = string.Empty;
        }
    }

    private void HandlePreviousCraftToolClicked()
    {
        SelectCraftToolOffset(-1);
    }

    private void HandleNextCraftToolClicked()
    {
        SelectCraftToolOffset(1);
    }

    private void SelectCraftToolOffset(int offset)
    {
        if (IsCrafting || _useRoutineState.IsUsingItem)
        {
            return;
        }

        RefreshCraftToolCandidates();

        if (_craftToolCandidates.Count <= 1)
        {
            RefreshCraftDetails();
            return;
        }

        int count = _craftToolCandidates.Count;
        _selectedCraftToolIndex = (_selectedCraftToolIndex + offset + count) % count;

        RefreshCraftDetails();
        RebuildCraftList();
    }

    private CraftToolCandidate? GetSelectedCraftTool()
    {
        if (_selectedCraftToolIndex < 0 || _selectedCraftToolIndex >= _craftToolCandidates.Count)
        {
            return null;
        }

        return _craftToolCandidates[_selectedCraftToolIndex];
    }

    private bool CanCraftListItem(CraftRecipe recipe)
    {
        if (_craftingService == null || recipe == null)
        {
            return false;
        }

        if (recipe == _selectedCraftRecipe)
        {
            return _craftingService.CanCraft(recipe, GetSelectedCraftTool());
        }

        if (!CraftingInventoryQuery.HasToolRequirement(recipe))
        {
            return _craftingService.CanCraft(recipe, null);
        }

        CraftingInventoryQuery.BuildToolCandidates(_inventoryController, recipe, _craftListToolCandidatesScratch);
        CraftToolCandidate? firstAvailableTool = _craftListToolCandidatesScratch.Count > 0 ? _craftListToolCandidatesScratch[0] : null;

        return _craftingService.CanCraft(recipe, firstAvailableTool);
    }

    private void HandleCraftActionClicked()
    {
        if (IsCrafting || _useRoutineState.IsUsingItem || _selectedCraftRecipe == null || _craftingService == null)
        {
            return;
        }

        CraftToolCandidate? selectedTool = GetSelectedCraftTool();

        if (!_craftingService.CanCraft(_selectedCraftRecipe, selectedTool))
        {
            RefreshCraftView();
            return;
        }

        _craftRoutine = StartCoroutine(CraftRoutine(_selectedCraftRecipe, selectedTool));
    }

    private IEnumerator CraftRoutine(CraftRecipe recipe, CraftToolCandidate? selectedTool)
    {
        BeginCraftingTimeAdvance();

        if (_craftActionButton != null)
        {
            _craftActionButton.interactable = false;
        }

        _useProgressModal?.Show(CraftProgressText);

        float duration = Mathf.Max(0.01f, _craftDurationSeconds);
        float elapsed = 0f;
        float advancedGameMinutes = 0f;

        try
        {
            while (elapsed < duration)
            {
                float deltaTime = Mathf.Min(Time.deltaTime, duration - elapsed);
                elapsed += deltaTime;

                float progress = Mathf.Clamp01(elapsed / duration);
                float targetAdvancedGameMinutes = recipe.GameMinutes * progress;
                float deltaGameMinutes = targetAdvancedGameMinutes - advancedGameMinutes;

                if (deltaGameMinutes > 0f)
                {
                    _craftGameTimeAdvancer?.AddGameMinutes(deltaGameMinutes);
                    advancedGameMinutes = targetAdvancedGameMinutes;
                }

                _useProgressModal?.UpdateProgress(progress, CraftProgressText);

                yield return null;
            }

            _useProgressModal?.Complete(CraftProgressText);
            _craftingService?.CompleteCraft(recipe, selectedTool);
        }
        finally
        {
            EndCraftingTimeAdvance();
        }

        _craftRoutine = null;
        _useProgressModal?.HideAndReset();

        RefreshCraftView();
    }

    private void BeginCraftingTimeAdvance()
    {
        if (_craftingAdvancingTime)
        {
            return;
        }

        _craftingAdvancingTime = true;

        if (_craftGameTimeRunController == null)
        {
            return;
        }

        _previousCraftGameTimeRunning = _craftGameTimeRunController.IsRunning;
        _restoreCraftGameTimeRunning = true;

        if (_previousCraftGameTimeRunning)
        {
            _craftGameTimeRunController.SetRunning(false);
        }
    }

    private void EndCraftingTimeAdvance()
    {
        if (!_craftingAdvancingTime)
        {
            return;
        }

        _craftingAdvancingTime = false;

        if (_restoreCraftGameTimeRunning && _craftGameTimeRunController != null)
        {
            _craftGameTimeRunController.SetRunning(_previousCraftGameTimeRunning);
        }

        _restoreCraftGameTimeRunning = false;
        _previousCraftGameTimeRunning = false;
    }

    private static string FormatCraftTime(float gameMinutes)
    {
        int totalMinutes = Mathf.Max(0, Mathf.RoundToInt(gameMinutes));
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        if (hours > 0 && minutes > 0)
        {
            return $"{hours}ч {minutes} мин";
        }

        if (hours > 0)
        {
            return $"{hours}ч";
        }

        return $"{minutes} мин";
    }

    private void ResolveCraftingReferences()
    {
        if (_inventoryRoot == null)
        {
            return;
        }

        _inventoryWindow ??= FindInventoryChild("Inventory Window")?.gameObject;
        _craftWindow ??= FindInventoryChild("Craft Window")?.gameObject;

        _inventoryTabButton ??= FindInventoryComponent<Button>("Buttons Holder/Inventory Button");
        _inventoryTabCanvasGroup ??= _inventoryTabButton != null ? _inventoryTabButton.GetComponent<CanvasGroup>() : null;

        _craftTabButton ??= FindInventoryComponent<Button>("Buttons Holder/Craft Button");
        _craftTabCanvasGroup ??= _craftTabButton != null ? _craftTabButton.GetComponent<CanvasGroup>() : null;

        _craftListRoot ??= FindInventoryChild("Craft Window/Craft Items Window/Scroll View/Viewport/Content");

        _craftItemIcon ??= FindInventoryComponent<Image>("Craft Window/Icon Image");
        _craftItemNameText ??= FindInventoryComponent<TMP_Text>("Craft Window/Name Text");
        _craftItemDescriptionText ??= FindInventoryComponent<TMP_Text>("Craft Window/Description Text");
        _craftRequirementsRoot ??= FindInventoryChild("Craft Window/Details Right Window/Item Holder");
        _craftDetailsRightWindow ??= FindInventoryChild("Craft Window/Details Right Window")?.gameObject;
        _craftTimeText ??= FindInventoryComponent<TMP_Text>("Craft Window/Details Right Window/Time Info/Time Text");
        _craftActionButton ??= FindInventoryComponent<Button>("Craft Window/Craft Button");

        _craftToolInfoRoot ??= FindInventoryChild("Craft Window/Details Right Window/Tool Info")?.gameObject;
        _craftToolItemViewRoot ??= FindInventoryChild("Craft Window/Details Right Window/Tool Info/Item View")?.gameObject;
        _craftNoToolTextRoot ??= FindInventoryChild("Craft Window/Details Right Window/Tool Info/No Tool Text")?.gameObject;
        _craftToolIconImage ??= FindInventoryComponent<Image>("Craft Window/Details Right Window/Tool Info/Item View/Icon");
        _craftToolNameText ??= FindInventoryComponent<TMP_Text>("Craft Window/Details Right Window/Tool Info/Item Name Text");
        _craftToolDurabilityText ??= FindInventoryComponent<TMP_Text>("Craft Window/Details Right Window/Tool Info/Stats Holder/Durability Text");
        _craftToolDurabilityIcon ??= FindInventoryComponent<Image>("Craft Window/Details Right Window/Tool Info/Stats Holder/Durability Icon");
        _craftToolStatsRoot ??= FindInventoryChild("Craft Window/Details Right Window/Tool Info/Stats Holder")?.gameObject;
        _craftToolLeftButton ??= FindOrCreateInventoryButton("Craft Window/Details Right Window/Tool Info/Item View/Left Button");
        _craftToolRightButton ??= FindOrCreateInventoryButton("Craft Window/Details Right Window/Tool Info/Item View/Right Button");
    }

    private Transform FindInventoryChild(string path)
    {
        if (_inventoryRoot == null || string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        return _inventoryRoot.transform.Find(path);
    }

    private T FindInventoryComponent<T>(string path) where T : Component
    {
        Transform child = FindInventoryChild(path);

        return child != null ? child.GetComponent<T>() : null;
    }

    private Button FindOrCreateInventoryButton(string path)
    {
        Transform child = FindInventoryChild(path);

        if (child == null)
        {
            return null;
        }

        Button button = child.GetComponent<Button>();

        if (button != null)
        {
            return button;
        }

        button = child.gameObject.AddComponent<Button>();
        button.targetGraphic = child.GetComponent<Graphic>();

        return button;
    }

    private static void SetActiveSafe(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private static void SetCanvasGroupAlpha(CanvasGroup canvasGroup, float alpha)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }
}
