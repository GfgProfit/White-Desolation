public static class PlayerInputResolver
{
    public static IPlayerInput Resolve(IPlayerInput current)
    {
        return current ?? new LegacyPlayerInput();
    }
}
