public sealed class InteractionInputReader
{
    private readonly IPlayerInput _playerInput;

    public InteractionInputReader(IPlayerInput playerInput)
    {
        _playerInput = playerInput;
    }

    public InteractionInputState Read()
    {
        if (_playerInput == null)
        {
            return InteractionInputState.Empty;
        }

        return new InteractionInputState(_playerInput.IsInteractPressed(), _playerInput.IsInteractDenied());
    }
}