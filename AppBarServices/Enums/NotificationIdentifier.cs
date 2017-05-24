// The enum 'NotificationIdentifier' is used by ...

namespace AppBarServices.Enums
{
    internal enum NotificationIdentifier
    {
        // Notifies an appbar that the taskbar's autohide or always-on-top state has changed — that is, the user 
        // has selected or cleared the "Always on top" or "Auto hide" check box on the taskbar's property sheet.
        ABN_STATECHANGE,
        // Notifies an appbar when an event has occurred that may affect the appbar's size and position.
        // Events include changes in the taskbar's size, position, and visibility state, as well as the addition,
        // removal, or resizing of another appbar on the same side of the screen.
        ABN_POSCHANGED,
        // Notifies an appbar when a full-screen application is opening or closing. This notification is sent
        // in the form of an application-defined message that is set by the ABM_NEW message.
        ABN_FULLSCREENAPP,
        // Notifies the system when an appbar's position has changed. An appbar should call this message in 
        // response to the WM_WINDOWPOSCHANGED message.
        ABN_WINDOWARRANGE
    }
}
