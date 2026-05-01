using UnityEngine;

public sealed partial class CrateContainer
{
    private const string SearchedInfoText = "Обыскано";
    private const string EmptyInfoText = "Пусто";

    public InteractionHoverInfo GetHoverInfo()
    {
        return new InteractionHoverInfo
        {
            InteractionText = _interactionText
        };
    }

    public bool TryGetExtraInfo(out string infoText)
    {
        infoText = string.Empty;

        if (!_searched)
        {
            return false;
        }

        infoText = HasItems ? SearchedInfoText : EmptyInfoText;
        return true;
    }

    public void Interact()
    {
        EnsureLootGenerated();

        CrateUIController uiController = ResolveUIController();

        if (uiController == null)
        {
            Debug.LogWarning($"{DebugPrefix} Cannot interact with '{name}' without CrateUIController.", this);
            return;
        }

        if (!_searched)
        {
            uiController.BeginSearch(this);
            return;
        }

        uiController.OpenCrate(this);
    }

    public void MarkSearched()
    {
        if (_searched)
        {
            return;
        }

        _searched = true;
        NotifyChanged();
    }

    private CrateUIController ResolveUIController()
    {
        if (_uiController != null)
        {
            return _uiController;
        }

        _uiController = CrateSceneReferenceResolver.FindSceneObject<CrateUIController>();

        if (_uiController == null)
        {
            GameObject controllerObject = new("Crate UI Controller");
            _uiController = controllerObject.AddComponent<CrateUIController>();
            RuntimeObjectInjector.Inject(controllerObject);
        }

        return _uiController;
    }
}
