// The struct 'AppBarData' is used by the function 'SHAppBarMessage' and the members being used 
// differ according to different 'MessageIdentifier' enum items being supplied to the function.

using System;
using System.Runtime.InteropServices;

using AppBarServices.Enums;

namespace AppBarServices.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct AppBarData
    {
        // The size of the struct in bytes. All messages use this member.
        internal int cbSize;
        // The handle to the appbar window. Not all messages use this member.
        internal IntPtr hWnd;
        // An application-defined message identifier. The application uses the specified identifier for
        // notification messages that it sends to the appbar identified by the hWnd member.
        // This member is used when sending the ABM_NEW message.
        internal int uCallbackMessage;
        // A value that specifies an edge of the screen. The following messages use this member:
        // ABM_GETAUTOHIDEBAR, ABM_SETAUTOHIDEBAR, ABM_GETAUTOHIDEBAREX, ABM_SETAUTOHIDEBAREX, ABM_QUERYPOS, ABM_SETPOS
        internal int uEdge;
        // The use depends on the message:
        //  - ABM_GETTASKBARPOS, ABM_QUERYPOS, ABM_SETPOS: The bounding rectangle, in screen coordinates, of an appbar or the Windows taskbar.
        //  - ABM_GETAUTOHIDEBAREX, ABM_SETAUTOHIDEBAREX: The monitor on which the operation is being performed. 
        //    This information can be retrieved through the GetMonitorInfo function.
        internal Rectangle rc;
        // A message-dependent value. This member is used with these messages: ABM_SETAUTOHIDEBAR, ABM_SETAUTOHIDEBAREX, ABM_SETSTATE
        internal IntPtr lParam;
    }
}
