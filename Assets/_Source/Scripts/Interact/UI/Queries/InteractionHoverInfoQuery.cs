public sealed class InteractionHoverInfoQuery
{
    public InteractionHoverInfo Build(InteractionTarget target)
    {
        InteractionHoverInfo info = InteractionHoverInfo.Empty;

        if (target.HasHoverInfo)
        {
            info = target.HoverInfo.GetHoverInfo();
        }

        MergeExtraInfo(ref info, target.ExtraInfo);

        return info;
    }

    private static void MergeExtraInfo(ref InteractionHoverInfo info, IInteractionExtraInfoProvider extraInfo)
    {
        if (extraInfo == null)
        {
            return;
        }

        if (info.HasInfoText)
        {
            return;
        }

        if (extraInfo.TryGetExtraInfo(out string extraText))
        {
            info.InfoText = extraText;
        }
    }
}