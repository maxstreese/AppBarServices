// The struct 'Rectangle' holds the values for the upper-left and lower-right corners of a window.
using System.Runtime.InteropServices;

namespace AppBarServices.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct Rectangle
    {
        internal long left;
        internal long top;
        internal long right;
        internal long bottom;
    }
}
