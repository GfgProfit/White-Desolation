using System.Collections;
using UnityEngine;

public partial class InventoryUIController
{
    private void HandleUseClicked()
    {
        if (_inventoryController == null || !_useRoutineState.CanStart)
        {
            return;
        }

        int selectedSlotIndex = _selectionState.SelectedSlotIndex;
        InventorySlot slot = _inventoryController.GetSlotAt(selectedSlotIndex);

        if (slot == null || slot.Item == null)
        {
            return;
        }

        ItemUseContext context = BuildItemUseContext();

        if (!ItemUseService.CanUseSlot(context, slot))
        {
            return;
        }

        if (!ItemUseService.TryBuildPlan(context, selectedSlotIndex, slot, out ItemUsePlan plan))
        {
            return;
        }

        _useRoutineState.TrackRoutine(StartCoroutine(ExecuteUseRoutine(plan)));
    }

    private ItemUseContext BuildItemUseContext()
    {
        return new ItemUseContext(_inventoryController, _playerNeedsController, _useDurationSeconds, _useRoutineState.IsUsingItem);
    }

    private IEnumerator ExecuteUseRoutine(ItemUsePlan plan)
    {
        _useRoutineState.BeginUse();
        _useProgressApplier?.Reset();

        RefreshDetails();

        _useProgressModal?.Show(plan.VerbText);

        float elapsed = 0f;

        while (elapsed < plan.Duration)
        {
            elapsed += Time.deltaTime;

            float progress = Mathf.Clamp01(elapsed / plan.Duration);

            _useProgressModal?.UpdateProgress(progress, plan.VerbText);
            _useProgressApplier?.ApplyProgress(plan, progress);

            yield return null;
        }

        _useProgressModal?.Complete(plan.VerbText);

        InventoryUseCompletionResult completionResult = _useCompletionService.Complete(plan, BuildItemUseContext());

        if (!completionResult.Success)
        {
            FinishUseRoutineState(true);
            yield break;
        }

        if (completionResult.HasNextPlan)
        {
            _useProgressModal?.HideAndReset();
            RefreshView();

            _useRoutineState.TrackRoutine(StartCoroutine(ExecuteUseRoutine(completionResult.NextPlan)));
            yield break;
        }

        FinishUseRoutineState(true);
    }

    private void FinishUseRoutineState(bool refreshView)
    {
        _useProgressModal?.HideAndReset();
        _useProgressApplier?.Reset();

        _useRoutineState.FinishUse();

        if (refreshView)
        {
            RefreshView();
        }
    }

    private void HandleDropOneClicked()
    {
        if (_inventoryController == null || _useRoutineState.IsUsingItem)
        {
            return;
        }

        int selectedSlotIndex = _selectionState.SelectedSlotIndex;
        InventorySlot slot = _inventoryController.GetSlotAt(selectedSlotIndex);

        if (slot == null || slot.Item == null)
        {
            return;
        }

        _inventoryController.TryRemoveFromSlot(selectedSlotIndex, 1);
    }
}