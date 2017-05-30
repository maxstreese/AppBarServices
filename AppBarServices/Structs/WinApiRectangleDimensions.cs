// The struct 'WinApiRectangleDimensions' holds the values for the position of a rectangle.

namespace AppBarServices.Structs
{
    internal struct WinApiRectangleDimensions
    {
        // The microsoft docs specify the members as C++ longs. It seems that C++ longs are C# ints.
        internal int x;
        internal int y;
        internal int width;
        internal int height;
    }
}
