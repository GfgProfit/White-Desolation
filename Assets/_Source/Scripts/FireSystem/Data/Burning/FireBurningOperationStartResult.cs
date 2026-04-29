public sealed class FireBurningOperationStartResult
{
    public bool Succeeded { get; }
    public FireBurningOperationExecution Execution { get; }
    public bool RequiresProgress => Succeeded && Execution != null;
    public bool CompletedImmediately => Succeeded && Execution == null;

    private FireBurningOperationStartResult(bool succeeded, FireBurningOperationExecution execution)
    {
        Succeeded = succeeded;
        Execution = execution;
    }

    public static FireBurningOperationStartResult Failed()
    {
        return new FireBurningOperationStartResult(false, null);
    }

    public static FireBurningOperationStartResult Completed()
    {
        return new FireBurningOperationStartResult(true, null);
    }

    public static FireBurningOperationStartResult Started(FireBurningOperationExecution execution)
    {
        return execution == null ? Failed() : new FireBurningOperationStartResult(true, execution);
    }
}
