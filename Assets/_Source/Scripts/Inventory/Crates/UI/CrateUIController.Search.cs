using System.Collections;
using UnityEngine;

public sealed partial class CrateUIController
{
    public void BeginSearch(CrateContainer crate)
    {
        if (crate == null)
        {
            return;
        }

        EnsureRuntimeReferences();
        AutoWireSceneReferences();

        CloseCrate();
        CloseBrowsing();
        StopSearchRoutine();

        _activeCrate = crate;
        _searchRoutine = StartCoroutine(SearchRoutine(crate));
    }

    private IEnumerator SearchRoutine(CrateContainer crate)
    {
        LockPlayerControls();
        _searchProgress.Show(_searchProgressLabel);

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, crate.SearchDurationSeconds);

        while (elapsed < duration)
        {
            if (_playerInput == null || !_playerInput.IsInteractHold())
            {
                CompleteSearchCancellation();
                yield break;
            }

            elapsed += Time.deltaTime;
            _searchProgress.UpdateProgress(Mathf.Clamp01(elapsed / duration), _searchProgressLabel);

            yield return null;
        }

        _searchProgress.Complete(_searchProgressLabel);
        _searchProgress.HideAndReset();
        _searchRoutine = null;

        crate.MarkSearched();
        UnlockPlayerControls();

        BeginBrowseSearchResults(crate);
    }

    private void StopSearchRoutine()
    {
        if (_searchRoutine == null)
        {
            return;
        }

        StopCoroutine(_searchRoutine);
        CompleteSearchCancellation();
    }

    private void CompleteSearchCancellation()
    {
        _searchRoutine = null;
        _searchProgress?.HideAndReset();
        UnlockPlayerControls();
    }
}
