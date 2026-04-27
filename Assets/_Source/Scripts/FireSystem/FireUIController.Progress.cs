using System.Collections;
using UnityEngine;

public partial class FireUIController
{
    private IEnumerator FireProgressRoutine(FireStartPlan plan, bool success, float targetFill)
    {
        float duration = Mathf.Max(0.01f, plan.StartDurationSeconds);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float fill = Mathf.Lerp(0f, targetFill, elapsed / duration);
            _progressView?.SetFill(fill);
            yield return null;
        }

        _progressView?.SetFill(targetFill);

        _completionService.Complete(plan, _currentSource, success);

        _startRoutine = null;

        CloseAll();
    }
}