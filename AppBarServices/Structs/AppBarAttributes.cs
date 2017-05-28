// The struct 'AppBarAttributes' holds the values of all attributes of the handled window that are related to
// the AppBar functionality and have to be used at multiple points in the AppBarHandler class.

using AppBarServices.Enums;

namespace AppBarServices.Structs
{
    internal struct AppBarAttributes
    {
        // Whether the AppBar is registered as an AutoHide AppBar. Can (or at least should) only be set to true
        // when the AppBar is already registered.
        internal bool isRegisteredAutoHide;
        // Whether the AppBar is registered. This must (or at least should) be set to true before isAutoHide is
        // to true.
        internal bool isRegistered;
        // Whether the AppBar is hidden or visible. Only used for an appBar that is registered as AutoHide.
        internal bool isHidden;
        // The edge the AppBar is bound to.
        internal ScreenEdge screenEdge;
        // One side of an AppBar is bound to a screen edge. The VisibleMargin determines how far away to the
        // opposite side of the screen the oppisite side of the AppBar is when it is visible. The value should
        // lie in the interval (0, 1] where 1 means, the AppBar spans across the complete screen.
        internal double visibleMargin;
        // One side of an AppBar is bound to a screen edge. The HiddenMargin determines how far away to the
        // opposite side of the screen the oppisite side of the AppBar is when it is hidden. The value should
        // lie in the interval (0, 1] where 1 means, the AppBar spans across the complete screen. While this
        // value can be set to as close to 0 as one would like, the minimal HiddenMargin that will be set by
        // the handler is 2 pixels.
        internal double hiddenMargin;
    }
}
