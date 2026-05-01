using UnityEngine;

public sealed class InteractionInspectSessionController
{
    private readonly PlayerControlLockSession _controlLockSession;

    public IInspectableInteractable Target { get; private set; }

    public bool IsOpen => Target != null;

    public InteractionInspectSessionController(object lockOwner, Behaviour[] disableWhileInspectOpen, GameObject[] objectDisableWhileInspectOpen)
    {
        _controlLockSession = PlayerControlLockService.CreateSession(lockOwner, disableWhileInspectOpen, objectDisableWhileInspectOpen);
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

        _controlLockSession.Release();
    }

    private void LockControls()
    {
        _controlLockSession.Lock();
    }

    private void UnlockControls()
    {
        _controlLockSession.Unlock();
    }
}
