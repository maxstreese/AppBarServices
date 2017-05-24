// The enum 'MessageIdentifier' is used by the function 'SHAppBarMessage' to specify what kind of message with regard to the AppBar
// the function is sending to the operating system. The data that is send via 'SHAppBarMessage' to the OS is in a struct parameter of type
// 'AppBarData'.

namespace AppBarServices.Enums
{
    internal enum MessageIdentifier
    {
        // Registers a new appbar and specifies the message identifier that the system should use to send notification messages to the appbar.
        ABM_NEW,
        // Unregisters an appbar, removing the bar from the system's internal list.
        ABM_REMOVE,
        // Requests a size and screen position for an appbar.
        ABM_QUERYPOS,
        // Sets the size and screen position of an appbar.
        ABM_SETPOS,
        // Retrieves the autohide and always-on-top states of the Windows taskbar.
        ABM_GETSTATE,
        // Retrieves the bounding rectangle of the Windows taskbar. Important note: Other areas might also not be visible to the user because of
        // third party objects (e.g. other AppBars). To retrieve the area of the screen not covered by both the taskbar and other app bars
        // use the GetMonitorInfo function.
        ABM_GETTASKBARPOS,
        // Notifies the system to activate or deactivate an appbar. The lParam member of the APPBARDATA pointed to by pData is set to TRUE
        // to activate or FALSE to deactivate.
        ABM_ACTIVATE,
        // Retrieves the handle to the autohide appbar associated with a particular edge of the screen.
        ABM_GETAUTOHIDEBAR,
        // Registers or unregisters an autohide appbar for an edge of the screen.
        ABM_SETAUTOHIDEBAR,
        // Notifies the system when an appbar's position has changed.
        ABM_WINDOWPOSCHANGED,
        // Sets the state of the appbar's autohide and always-on-top attributes.
        ABM_SETSTATE,
        // Retrieves the handle to the autohide appbar associated with a particular edge of a particular monitor.
        ABM_GETAUTOHIDEBAREX,
        // Registers or unregisters an autohide appbar for an edge of a particular monitor.
        ABM_SETAUTOHIDEBAREX
    }
}
