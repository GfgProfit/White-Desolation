public interface IInteractionExtraInfoProvider
{
    bool TryGetExtraInfo(out string infoText);
}