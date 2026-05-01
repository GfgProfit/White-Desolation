public sealed partial class CrateUIController
{
    private void LockPlayerControls()
    {
        EnsureControlLockSession();
        _controlLockSession.Lock();
    }

    private void UnlockPlayerControls()
    {
        _controlLockSession?.Unlock();
    }

    private void ReleasePlayerControls()
    {
        _controlLockSession?.Release();
    }

    private void EnsureControlLockSession()
    {
        _controlLockSession ??= PlayerControlLockService.CreateSession(this, _disableWhileOpen, _objectDisableWhileOpen);
    }
}
