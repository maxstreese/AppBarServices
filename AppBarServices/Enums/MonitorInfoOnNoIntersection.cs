// The enum 'MonitorFromRectOnNoIntersection' can be used to specify which monitor handle the function
// 'MonitorFromRect' should return in case the provided 'WinApiRectangle' parameter does not intersect
// any display monitor.

namespace AppBarServices.Enums
{
    internal enum MonitorInfoOnNoIntersection
    {
        MONITOR_DEFAULTTONULL,
        MONITOR_DEFAULTTOPRIMARY,
        MONITOR_DEFAULTTONEAREST
    }
}
