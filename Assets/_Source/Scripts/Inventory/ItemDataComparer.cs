public static class ItemDataComparer
{
    public static bool AreSame(ItemData a, ItemData b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(a.Id) && !string.IsNullOrWhiteSpace(b.Id))
        {
            return a.Id == b.Id;
        }

        return ReferenceEquals(a, b);
    }
}