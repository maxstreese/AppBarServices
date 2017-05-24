// The struct 'AppBarAttributes' holds the values of all attributes of the handled window
// that are related to the AppBar functionality and have to be used at multiple points in the AppBarHandler class.

using AppBarServices.Enums;

namespace AppBarServices.Structs
{
    internal struct AppBarAttributes
    {
        internal bool isAutoHide;
        internal bool isRegistered;
        internal ScreenEdge screenEdge;
        internal double margin;
    }
}
