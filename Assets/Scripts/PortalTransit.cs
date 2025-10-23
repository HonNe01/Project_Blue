public static class PortalTransit
{
    public static bool HasPending { get; private set; }             // 대기 중인 포탈 이동 정보 존재 여부
    public static Portal.PortalType Origin { get; private set; }    // 이동할 포탈
    public static Portal.PortalType pathA { get; private set; }     // 경로 포탈 A
    public static Portal.PortalType pathB { get; private set; }     // 경로 포탈 B
    public static int RouteId { get; private set; }                 // 경로 ID 

    private static bool _consumed;


    public static void Set(Portal.PortalType origin, Portal.PortalType pathA, Portal.PortalType pathB, int routeId)
    {
        // 순서 상관없이 저장
        if ((int)pathA <= (int)pathB)
        {
            PortalTransit.pathA = pathA;
            PortalTransit.pathB = pathB;
        }
        else
        {
            PortalTransit.pathA = pathB;
            PortalTransit.pathB = pathA;
        }

        // 저장
        Origin = origin;
        RouteId = routeId;

        // 초기화
        _consumed = false;
        HasPending = true;
    }

    public static bool TryConsume(Portal.PortalType thisScene, Portal.PortalType endA, Portal.PortalType endB, int routeId)
    {
        if (!HasPending || _consumed) return false;

        // 경로 일치 확인
        Portal.PortalType checkA = endA, checkB = endB;
        if ((int)checkA > (int)checkB)
        {
            var t = checkA;
            checkA = checkB;
            checkB = t;
        }

        if (pathA != checkA || pathB != checkB || RouteId != routeId) return false;
        if (RouteId != routeId) return false;

        // 목적지 계산
        var destination = (thisScene == endA) ? pathB : (thisScene == endB) ? pathA : (Portal.PortalType)(-1);
        if (destination != Origin) return false;

        _consumed = true;
        HasPending = false;

        // 초기화
        Origin = default;
        pathA = default;
        pathB = default;
        RouteId = 0;
        return true;
    }

    public static void Clear()
    {
        HasPending = false;
        _consumed = false;

        // 초기화
        Origin = default;
        pathA = default;
        pathB = default;
        RouteId = 0;
    }
}
