// The struct 'WinApiRectangle' holds the values for the upper-left and lower-right corners of a window.

namespace AppBarServices.Structs
{
    internal struct WinApiRectangle
    {
        // The microsoft docs specify the members as C++ longs. It seems that C++ longs are C# ints, because if these members
        // would be defined as long, SHAppBarMessage would not work (e.g. return false for an otherwise perfectly fine ABM_NEW message).
        internal int left;
        internal int top;
        internal int right;
        internal int bottom;
    }
}
