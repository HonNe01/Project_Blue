public static class PortalTransit
{
    public static bool HasPending { get; private set; }
    public static Portal.PortalType NextFrom { get; private set; }


    public static void SetNextFrom(Portal.PortalType from)
    {
        NextFrom = from;
        HasPending = true;
    }

    public static void Clear()
    {
        HasPending = false;
    }
}
