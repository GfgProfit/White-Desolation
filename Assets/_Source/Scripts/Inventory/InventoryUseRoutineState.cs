using UnityEngine;

public sealed class InventoryUseRoutineState
{
    public bool IsUsingItem { get; private set; }
    public Coroutine Routine { get; private set; }

    public bool CanStart => !IsUsingItem && Routine == null;

    public void TrackRoutine(Coroutine routine)
    {
        Routine = routine;
    }

    public void BeginUse()
    {
        IsUsingItem = true;
    }

    public void FinishUse()
    {
        IsUsingItem = false;
        Routine = null;
    }

    public void StopAndReset(MonoBehaviour owner)
    {
        if (owner != null && Routine != null)
        {
            owner.StopCoroutine(Routine);
        }

        FinishUse();
    }
}