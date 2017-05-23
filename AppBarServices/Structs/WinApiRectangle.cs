// The struct 'Rectangle' holds the values for the upper-left and lower-right corners of a window.

namespace AppBarServices.Structs
{
    internal struct WinApiRectangle
    {
        // The microsoft docs specify the members as C++ longs. This is either wrong or C++ longs are C# ints. If these members
        // would be defined as long, SHAppBarMessage would not work (e.g. return false for an otherwise perfectly fine ABM_NEW message).
        internal int left;
        internal int top;
        internal int right;
        internal int bottom;
    }
}
