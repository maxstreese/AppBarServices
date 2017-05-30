// The struct 'WinApiRectanglePoints' holds the values for the position of a rectangle.

namespace AppBarServices.Structs
{
    internal struct WinApiRectanglePoints
    {
        // The microsoft docs specify the members as C++ longs. It seems that C++ longs are C# ints, because if these members
        // would be defined as long, SHAppBarMessage would not work (e.g. return false for an otherwise perfectly fine ABM_NEW message).
        internal int left;
        internal int top;
        internal int right;
        internal int bottom;
    }
}
