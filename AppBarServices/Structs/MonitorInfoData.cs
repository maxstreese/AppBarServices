// The struct 'MonitorInfoData' contains information about a display monitor. It is used by the 'GetMonitorInfo' function
// to retrieve information about a specified monitor from the operating system.

namespace AppBarServices.Structs
{
    internal struct MonitorInfoData
    {
        internal int cbSize;
        internal WinApiRectangle rcMonitor;
        internal WinApiRectangle rcWork;
        internal int dwFlags;
    }
}
