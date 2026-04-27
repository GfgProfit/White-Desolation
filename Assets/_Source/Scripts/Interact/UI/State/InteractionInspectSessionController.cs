using UnityEngine;

public sealed class InteractionInspectSessionController
{
    private readonly object _lockOwner;
    private readonly Behaviour[] _disableWhileInspectOpen;
    private readonly GameObject[] _objectDisableWhileInspectOpen;

    public IInspectableInteractable Target { get; private set; }

    public bool IsOpen => Target != null;

    public InteractionInspectSessionController(object lockOwner, Behaviour[] disableWhileInspectOpen, GameObject[] objectDisableWhileInspectOpen)
    {
        _lockOwner = lockOwner;
        _disableWhileInspectOpen = disableWhileInspectOpen;
        _objectDisableWhileInspectOpen = objectDisableWhileInspectOpen;
    }

    public bool Open(IInspectableInteractable target)
    {
        if (target == null || !target.CanInspect)
        {
            Close();
            return false;
        }

        Target = target;
        LockControls();

        return true;
    }

    public void Close()
    {
        Target = null;
        UnlockControls();
    }

    public void Release()
    {
        Target = null;

        if (_lockOwner != null)
        {
            PlayerControlLockService.ReleaseOwner(_lockOwner);
        }
    }

    private void LockControls()
    {
        if (_lockOwner == null)
        {
            return;
        }

        PlayerControlLockService.Lock(_lockOwner, _disableWhileInspectOpen, _objectDisableWhileInspectOpen);
    }

    private void UnlockControls()
    {
        if (_lockOwner == null)
        {
            return;
        }

        PlayerControlLockService.Unlock(_lockOwner, _disableWhileInspectOpen, _objectDisableWhileInspectOpen);
    }
}