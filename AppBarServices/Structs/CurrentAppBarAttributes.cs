using AppBarServices.Enums;

namespace AppBarServices.Structs
{
    /// <summary>
    /// Holds the values of all attributes of the handled window that are related to the AppBar functionality
    /// as they are at any particular point in time.
    /// </summary>
    internal struct CurrentAppBarAttributes
    {
        /// <summary>
        /// Whether the AppBar is registered as an AutoHide AppBar.
        /// </summary>
        /// <remarks>
        /// Can (or at least should) only be set to true when the AppBar is already registered.
        /// </remarks>
        internal bool isRegisteredAutoHide;
        
        /// <summary>
        /// Whether the AppBar is registered.
        /// </summary>
        /// <remarks>
        /// This must (or at least should) be set to true before isAutoHide is to true.
        /// </remarks>
        internal bool isRegistered;

        /// <summary>
        /// Whether the AppBar is hidden or visible.
        /// </summary>
        /// <remarks>
        /// Only used when the AppBar is an AutoHide AppBar (i.e. is registered as AutoHide).
        /// </remarks>
        internal bool isHidden;
        
        /// <summary>
        /// The edge of the screen the AppBar is bound to.
        /// </summary>
        internal ScreenEdge screenEdge;
        
        /// <summary>
        /// One side of an AppBar is bound to a screen edge. The VisibleMargin determines how far away to the
        /// opposite side of the screen the oppisite side of the AppBar is when it is visible.
        /// </summary>
        /// <remarks>
        /// The value should lie in the interval (0, 1] where 1 means, the AppBar spans across the complete
        /// screen. While this value can be set to as close to 0 as one would like, the minimal HiddenMargin
        /// that will be set by the handler is 2 pixels.
        /// </remarks>
        internal double visibleMargin;

        /// <summary>
        /// One side of an AppBar is bound to a screen edge. The HiddenMargin determines how far away to the
        /// opposite side of the screen the oppisite side of the AppBar is when it is hidden.
        /// </summary>
        /// <remarks>
        /// The value should lie in the interval (0, 1] where 1 means, the AppBar spans across the complete
        /// screen. While this value can be set to as close to 0 as one would like, the minimal HiddenMargin
        /// that will be set by the handler is 2 pixels.
        /// </remarks>
        internal double hiddenMargin;
        
        /// <summary>
        /// The position in pixels of the AppBar.
        /// </summary>
        internal WinApiRectanglePoints windowRectangle;

        // The monitor the AppBar is on.
        /// <summary>
        /// Information about the monitor the AppBar is currently on.
        /// </summary>
        internal MonitorInfoData monitorPlacedOn;
    }
}
