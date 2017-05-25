// The struct 'WPFWindowAttributes' holds the values of all attributes of the handled window that
// change when the window becomes an AppBar and should be restored to their original state once the
// window becomes a standard window again (unregisters as an AppBar). It also holds the top, left,
// height and width values (in pixels) of the screen the handled window was on before it became an AppBar.
// These are needed to reposition the window to its original position in an orderly fashion in a 
// multi monitor setup where monitor parameters (e.g. number of monitors or resolution) might change
// while the handled window is an AppBar.

using System.Windows;

namespace AppBarServices.Structs
{
    internal struct WPFWindowAttributes
    {
        internal double windowTop;
        internal double windowHeight;
        internal double windowLeft;
        internal double windowWidth;

        internal int monitorTop;
        internal int monitorLeft;
        internal int monitorHeight;
        internal int monitorWidth;

        internal WindowStyle windowStyle;
        internal ResizeMode resizeMode;
        internal WindowState windowState;
    }
}
