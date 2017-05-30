// The struct 'WinApiPoint' represents a point like the WinApi stores it.

namespace AppBarServices.Structs
{
    internal struct WinApiPoint
    {
        // The microsoft docs specify the members as C++ longs. It seems that C++ longs are C# ints.
        internal int x;
        internal int y;
    }
}
