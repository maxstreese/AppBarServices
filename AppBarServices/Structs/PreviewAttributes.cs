using System.Windows.Media;

namespace AppBarServices.Structs
{
    /// <summary>
    /// Captures the state necessary for the PreviewToSnap capability of the AppBarHandler (i.e. show a preview window for the AppBar
    /// when the program user drags the window to an edge of a screen and snap the AppBar to that edge if the program user lets go
    /// of the left mouse button while the preview window is shown.
    /// </summary>
    public struct PreviewAttributes
    {
        /// <summary>
        /// Indicates whether PreviewToSnap is enabled or not.
        /// </summary>
        public bool doPreviewToSnap;
        /// <summary>
        /// Defines the margin in percent (of pixels) that the mouse must be in in order for the preview window to show itself. 
        /// </summary>
        public double previewToSnapMargin;
        /// <summary>
        /// Defines the opacity of the preview window. 0 is invisible and 1 completely opaque.
        /// </summary>
        public double windowOpacity;
        /// <summary>
        /// Defines the background color of the preview window as a System.Windows.Media.Color type.
        /// </summary>
        public Color windowBackgroundColor;
        /// <summary>
        /// Defines the border color of the preview window as a System.Windows.Media.Color type.
        /// </summary>
        public Color windowBorderColor;
        /// <summary>
        /// Defines the thickness of the preview window border.
        /// </summary>
        public double windowBorderThickness;
    }
}
