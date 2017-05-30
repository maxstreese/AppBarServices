// The struct 'MonitorInfoData' contains information about a display monitor. It is used by the 'GetMonitorInfo' function
// to retrieve information about a specified monitor from the operating system.

namespace AppBarServices.Structs
{
    internal struct MonitorInfoData
    {
        internal int cbSize;
        internal WinApiRectanglePoints rcMonitor;
        internal WinApiRectanglePoints rcWork;
        internal int dwFlags;
    }
}
