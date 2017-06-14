using AppBarServices.Enums;

namespace AppBarServices.Structs
{
    /// <summary>
    /// Holds the values of all AppBar related attributes the library user must provide when initializing the AppBarHandler.
    /// </summary>
    public struct DesiredAppBarAttributes
    {
        /// <summary>
        /// Whether ór not the AppBar should register.
        /// </summary>
        public bool doRegister;
        /// <summary>
        /// Whether or not the AppBar should AutoHide when placed.
        /// </summary>
        public bool doAutoHide;
        /// <summary>
        /// The screen edge the AppBar should be bound to.
        /// </summary>
        public ScreenEdge screenEdge;
        /// <summary>
        /// One side of an AppBar is bound to a screen edge. The VisibleMargin determines how far away to the
        /// opposite side of the screen the oppisite side of the AppBar is when it is visible. The value should
        /// lie in the interval (0, 1] where 1 means, the AppBar spans across the complete screen.
        /// </summary>
        public double visibleMargin;
        /// <summary>
        /// One side of an AppBar is bound to a screen edge. The HiddenMargin determines how far away to the
        /// opposite side of the screen the oppisite side of the AppBar is when it is hidden. The value should
        /// lie in the interval (0, 1] where 1 means, the AppBar spans across the complete screen. While this
        /// value can be set to as close to 0 as one would like, the minimal HiddenMargin that will be set by
        /// the handler is 2 pixels.
        /// </summary>
        public double hiddenMargin;
    }
}
