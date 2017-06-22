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
        internal bool doRegister;

        /// <summary>
        /// Whether or not AutoHide is the default setting for the AppBar.
        /// </summary>
        /// <remarks>
        /// This member is similar to the member doAutoHide but not the same. This member is used to decide whether the
        /// AppBar should be placed as AutoHide whenever it is placed again, whereas doAutoHide is used internally by
        /// method within the AppBarHandler to decide whether at a particular moment in time the AppBar should get
        /// registered as AutoHide or not (i.e. unregistered).
        /// </remarks>
        public bool AutoHideIsDefault;
        
        /// <summary>
        /// Whether or not the AppBar should AutoHide when placed.
        /// </summary>
        internal bool doAutoHide;
        
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

        /// <summary>
        /// The monitor the handled window should be placed on. Only set internally by the AppBarHandler.
        /// </summary>
        /// <remarks>
        /// The monitor on which the handled window should be placed depends on the method that is used to
        /// place the AppBar.
        /// </remarks>
        internal MonitorInfoData monitorToPlaceOn;
    }
}
