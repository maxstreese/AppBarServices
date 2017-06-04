namespace AppBarServices.Structs
{
    /// <summary>
    /// Represents a point in the way the WinApi uses it. Used for convenience and for communication with the operating system.
    /// </summary>
    public struct WinApiPoint
    {
        /// <summary>
        /// X-coordinate of the point in pixels. 
        /// </summary>
        public int x;
        /// <summary>
        /// Y-coordinate of the point in pixels.
        /// </summary>
        public int y;
    }
}
