// The struct 'WPFWindowAttributes' holds the values of all attributes of the handled window that
// change when the window becomes an AppBar and should be restored to their original state once the
// window becomes a standard window again (unregisters as an AppBar).

using System.Windows;

namespace AppBarServices.Structs
{
    internal struct WPFWindowAttributes
    {
        internal double top;
        internal double height;
        internal double left;
        internal double width;
        internal WindowStyle windowStyle;
        internal ResizeMode resizeMode;
        internal WindowState windowState;
    }
}
