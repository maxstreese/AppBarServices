// The class 'AppBarHandler' implements all of the logic of 'AppBarServices'. It is the only class defined with the public modifier
// and its intended use is for a WPF window to define it as a member and call its public methods in order to implement an AppBar. 

using System.Diagnostics;

using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

using AppBarServices.Enums;
using AppBarServices.Structs;

namespace AppBarServices
{
    public class AppBarHandler
    {
        #region Fields
        // The window which the AppBarHandler is handling.
        private Window _windowToHandle;
        // Represents the _windowToHandle as a WinApi window. Used for WinApi interoperability.
        private HwndSource _windowSource;
        // To receive notifications from the OS, the AppBar window needs to provide a CallbackID.
        private int _callbackID;
        // The attributes of the _windowToHandle before it became and AppBar.
        private WPFWindowAttributes _originalWindowAttributes;
        // The attributes of the AppBar. This should always represent the current attributes.
        private AppBarAttributes _currentAppBarAttributes;
        #endregion


        #region Properties
        // Gets _currentAppBarAttributes.isRegistered.
        public bool AppBarIsRegistered
        {
            get { return _currentAppBarAttributes.isRegistered; }
        }

        // Gets or sets _currentAppBarAttributes.isRegisteredAutoHide.
        public bool AppBarIsRegisteredAutoHide
        {
            get { return _currentAppBarAttributes.isRegisteredAutoHide; }
            set { throw new NotImplementedException(); }
        }

        // Gets or sets the margin of the AppBar that is used when it is not hidden.
        public double AppBarVisibleMargin
        {
            get { return _currentAppBarAttributes.visibleMargin; }
            set { throw new NotImplementedException(); }
        }

        // Gets or sets the margin of the AppBar that is used when it is hidden. This value is only
        // used for an AutoHide AppBar.
        public double AppBarHiddenMargin
        {
            get { return _currentAppBarAttributes.hiddenMargin; }
            set { throw new NotImplementedException(); }
        }

        // Gets the value of _callbackID.
        public int WindowCallbackID
        {
            get { return _callbackID; }
        }
        #endregion


        #region Constructors
        public AppBarHandler(Window windowToHandle)
        {
            // Sets the CallbackID field to a unique value based on the datetime when the handler is initialized.
            _callbackID = (int)RegisterWindowMessage(String.Format("AppBarHandler_{0}", DateTime.Now.Ticks));
            
            _windowToHandle = windowToHandle;
        }
        #endregion


        #region Public Methods
        // Registers the WindowToHandle as an AppBar and places it to the specified position.
        public bool PlaceAppBar(bool isAutoHide, ScreenEdge screenEdge, double visibleMargin, double hiddenMargin = 0.01)
        {
            if (!_currentAppBarAttributes.isRegistered)
            {

                if (HandleAppBarNew())
                {
                    // Save all of the attributes needed to restore the window to its original position once
                    // it unregisters as an AppBar.
                    SaveRestoreOriginalWindowAttributes(doSave: true);

                    // Set the AppBarAttributes that are needed for both, a normal as well as an AutoHide AppBar.
                    _currentAppBarAttributes.screenEdge = screenEdge;
                    _currentAppBarAttributes.visibleMargin = visibleMargin;

                    if (!isAutoHide)
                    {
                        HandleAppBarQueryPosSetPos();
                    }
                    else
                    {

                    }

                    // The AppBar should have no borders at all.
                    _windowToHandle.WindowStyle = WindowStyle.None;
                    _windowToHandle.ResizeMode = ResizeMode.NoResize;

                    // Trying to move the _windowToHandle while it is in the maximized state doesn't work.
                    _windowToHandle.WindowState = WindowState.Normal;

                    // Setting the handled window to be topmost ensures that it stays in front even if it is deactivated
                    // (pressing WIN+G does not minimize the window anymore with this set to true).
                    _windowToHandle.Topmost = true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        // Moves _windowToHandle if it is already an AppBar
        public bool MoveAppBar(ScreenEdge screenEdge, double margin)
        {
            if (_currentAppBarAttributes.isRegistered)
            {
                _currentAppBarAttributes.screenEdge = screenEdge;
                _currentAppBarAttributes.visibleMargin = margin;

                HandleAppBarQueryPosSetPos();
            }
            else
            {
                return false;
            }

            return true;
        }

        // Removes the AppBar and sets the WindowToHandle back to its position before it became an AppBar.
        public void RemoveAppBar()
        {
            if (_currentAppBarAttributes.isRegistered)
            {
                // First check if the AppBar is registered as AutoHide and if so unregister. 
                if (_currentAppBarAttributes.isRegisteredAutoHide)
                {
                    HandleSetAutoHideBarX(doRegister: false);
                }

                // Then unregister the AppBar.
                HandleAppBarRemove();

                // Finally restore the original window attributes.
                SaveRestoreOriginalWindowAttributes(doSave: false);
            }
        }
        #endregion


        #region Methods to encapsulate the interaction with the operating system
        // Gets information in the form of a MonitorInfoData struct of the monitor that has the largest area of intersection with
        // the provided windowRectangle parameter. This method encapsulates the WinApi functions GetMonitorInfo and MonitorFromRect.
        internal MonitorInfoData HandleGetMonitorInfoFromRect(WinApiRectangle windowRectangle)
        {
            MonitorInfoData monitorInfoData = new MonitorInfoData();
            monitorInfoData.cbSize = Marshal.SizeOf(monitorInfoData);

            IntPtr monitorHandle;
            monitorHandle = MonitorFromRect(ref windowRectangle, (int)MonitorFromRectOnNoIntersection.MONITOR_DEFAULTTOPRIMARY);
            GetMonitorInfo(monitorHandle, ref monitorInfoData);

            return monitorInfoData;
        }

        // Registers the AppBar with the operating system (i.e. calls SHAppBarData with the MessageIdentifier ABM_NEW).
        private bool HandleAppBarNew()
        {
            // Specifying the AppBarData to be supplied to the SHAppBarMessage function.
            AppBarData appBarData = new AppBarData();
            appBarData.cbSize = Marshal.SizeOf(appBarData);
            appBarData.hWnd = _windowSource.Handle;
            appBarData.uCallbackMessage = _callbackID;

            // Call the SHAppBarMessage function and let it do its magic. If it fails return false.
            if (SHAppBarMessage((int)MessageIdentifier.ABM_NEW, ref appBarData) == 0)
            {
                return false;
            }

            // Since the AppBar is now registered, it must receive certain notifications and handle them via WindowProc.
            // Therefore a hook is added to the HwndSource object of the WPF window.
            _windowSource.AddHook(new HwndSourceHook(ProcessWinApiMessages));

            _currentAppBarAttributes.isRegistered = true;
            return true;
        }

        // Works out the position of the AppBar, reserves the space (i.e. calls SHAppBarData with the MessageIdentifiers ABM_QUERYPOS and ABM_SETPOS)
        // and moves the WPF window to the reserved space.
        private void HandleAppBarQueryPosSetPos()
        {
            // Specifying the AppBarData to be supplied to the SHAppBarMessage function.
            AppBarData appBarData = new AppBarData();
            appBarData.cbSize = Marshal.SizeOf(appBarData);
            appBarData.hWnd = _windowSource.Handle;
            appBarData.uEdge = (int)_currentAppBarAttributes.screenEdge;

            // Get the rectangle information of the monitor the handled window is currently on
            // (either as an already set AppBar or as a normal window).
            WinApiRectangle windowRectangle = new WinApiRectangle();
            windowRectangle.left = (int)_windowToHandle.Left;
            windowRectangle.top = (int)_windowToHandle.Top;
            windowRectangle.right = (int)(_windowToHandle.Left + _windowToHandle.Width);
            windowRectangle.bottom = (int)(_windowToHandle.Top + _windowToHandle.Height);
            MonitorInfoData currentMonitor = HandleGetMonitorInfoFromRect(windowRectangle);

            // The rectangle that the caller wants the AppBar to have. This variable is used to check whether the AppBar rectangle set by the
            // operating system is the same width- or height-wise (depending on the defined screen edge) as the rectangle desired by the caller.
            // Only if that is the case will the AppBar be set in the end.
            WinApiRectangle desiredRectangle = new WinApiRectangle();

            // Specify the dimensions of the AppBar based on the '_currentAppBarAttributes.screenEdge' and '_currentAppBarAttributes.margin' values.
            SetInitialRectangle(ref appBarData, ref desiredRectangle, currentMonitor);

            // Query the position where the AppBar should go and let the operating system adjust it for any obstacles.
            // Then take the adjusted values and correct them to represent the desired width or height value
            // (depending on the defined screen edge).
            SHAppBarMessage((int)MessageIdentifier.ABM_QUERYPOS, ref appBarData);
            AdjustQueriedAppBarDataRectangle(ref appBarData, desiredRectangle, currentMonitor);

            // Reserve the position for the AppBar and then check if the position set by the operating system is equal in
            // width or height (depending on the defined screen edge) to the desired width or height value. If that is the
            // case place the WPF window to that position. If it isn't remove the AppBar.
            uint testReturn;
            testReturn = SHAppBarMessage((int)MessageIdentifier.ABM_SETPOS, ref appBarData);
            if (AppBarDataRectangleIsDesired(appBarData, desiredRectangle, currentMonitor) == true)
            {
                _windowToHandle.Top = appBarData.rc.top;
                _windowToHandle.Left = appBarData.rc.left;
                _windowToHandle.Width = appBarData.rc.right - appBarData.rc.left;
                _windowToHandle.Height = appBarData.rc.bottom - appBarData.rc.top;
            }
            else
            {
                RemoveAppBar();
            }
        }

        // Removes the AppBar from the operating system.
        private void HandleAppBarRemove()
        {
            // Specifying the AppBarData to be supplied to the SHAppBarMessage function.
            AppBarData appBarData = new AppBarData();
            appBarData.cbSize = Marshal.SizeOf(appBarData);
            appBarData.hWnd = _windowSource.Handle;

            // Call the SHAppBarMessage function to remove the AppBar from the operating system.
            SHAppBarMessage((int)MessageIdentifier.ABM_REMOVE, ref appBarData);

            // Since the AppBar is not registered any longer, no messages from the operating system should receive special treatment anymore.
            // Therefore the hook is removed from the HwndSource object of the WPF window.
            _windowSource.RemoveHook(new HwndSourceHook(ProcessWinApiMessages));

            _currentAppBarAttributes.isRegistered = false;
        }

        // Registers and positions or unregisters an AutoHide AppBar (i.e. calls SHAppBarData with the MessageIdentifier ABM_SETAUTOHIDEBAREX).
        
        private bool HandleSetAutoHideBarX(bool doRegister)
        {
            // Specifying the AppBarData to be supplied to the SHAppBarMessage function part 1.
            AppBarData appBarData = new AppBarData();
            appBarData.cbSize = Marshal.SizeOf(appBarData);
            appBarData.lParam = Convert.ToInt32(doRegister);
            appBarData.uEdge = (int)_currentAppBarAttributes.screenEdge;
            appBarData.hWnd = _windowSource.Handle;

            // Get the rectangle information of the monitor the handled window is currently on
            // (either as an already set AppBar or as a normal window).
            WinApiRectangle windowRectangle = new WinApiRectangle();
            windowRectangle.left = (int)_windowToHandle.Left;
            windowRectangle.top = (int)_windowToHandle.Top;
            windowRectangle.right = (int)(_windowToHandle.Left + _windowToHandle.Width);
            windowRectangle.bottom = (int)(_windowToHandle.Top + _windowToHandle.Height);
            MonitorInfoData currentMonitor = HandleGetMonitorInfoFromRect(windowRectangle);

            // Specifying the AppBarData to be supplied to the SHAppBarMessage function part 2.
            appBarData.rc.left = currentMonitor.rcMonitor.left;
            appBarData.rc.top = currentMonitor.rcMonitor.top;
            appBarData.rc.right = currentMonitor.rcMonitor.right;
            appBarData.rc.bottom = currentMonitor.rcMonitor.bottom;

            // Send the ABM_SETAUTOHIDEBAREX message to the operating system and check if it succeded. If not, return false.
            int didRegister;
            didRegister = (int)SHAppBarMessage((int)MessageIdentifier.ABM_SETAUTOHIDEBAREX, ref appBarData);
            if (didRegister == 0)
            {
                return false;
            }

            // The WinApi function was successful, so the AppBar either was registered as AutoHide and now isn't anymore,
            // or was not registered as AutoHide and now is.
            if (!_currentAppBarAttributes.isRegisteredAutoHide)
            {
                _currentAppBarAttributes.isRegisteredAutoHide = true;
            }
            else
            {
                _currentAppBarAttributes.isRegisteredAutoHide = false;
            }
            
            return true;
        }


        // Processes window messages send by the operating system. This is a callback function that requires a hook on the
        // Win32 representation of the _windowToHandle (HwndSource object). This hook is added upon registration of the AppBar
        // and removed upon removal of the AppBar. This is the implementation of the placeholder function named WindowProc
        // (use that name to look it up in the microsoft docs).
        private IntPtr ProcessWinApiMessages(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (uMsg == _callbackID)
            {
                if (wParam.ToInt32() == (int)NotificationIdentifier.ABN_POSCHANGED)
                {
                    HandleAppBarQueryPosSetPos();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }
        #endregion


        #region Helper Methods
        private void SaveRestoreOriginalWindowAttributes(bool doSave)
        {
            if (doSave)
            {
                _originalWindowAttributes.windowTop = _windowToHandle.Top;
                _originalWindowAttributes.windowHeight = _windowToHandle.Height;
                _originalWindowAttributes.windowLeft = _windowToHandle.Left;
                _originalWindowAttributes.windowWidth = _windowToHandle.Width;

                _originalWindowAttributes.topMost = _windowToHandle.Topmost;
                _originalWindowAttributes.windowStyle = _windowToHandle.WindowStyle;
                _originalWindowAttributes.resizeMode = _windowToHandle.ResizeMode;
                _originalWindowAttributes.windowState = _windowToHandle.WindowState;
                
                WinApiRectangle windowRectangle = new WinApiRectangle();
                windowRectangle.left = (int)_originalWindowAttributes.windowLeft;
                windowRectangle.top = (int)_originalWindowAttributes.windowTop;
                windowRectangle.right = (int)(_originalWindowAttributes.windowLeft + _originalWindowAttributes.windowWidth);
                windowRectangle.bottom = (int)(_originalWindowAttributes.windowTop + _originalWindowAttributes.windowHeight);
                MonitorInfoData originalMonitor = HandleGetMonitorInfoFromRect(windowRectangle);

                _originalWindowAttributes.monitorTop = originalMonitor.rcMonitor.top;
                _originalWindowAttributes.monitorHeight = originalMonitor.rcMonitor.bottom - originalMonitor.rcMonitor.top;
                _originalWindowAttributes.monitorLeft = originalMonitor.rcMonitor.left;
                _originalWindowAttributes.monitorWidth = originalMonitor.rcMonitor.right - originalMonitor.rcMonitor.left;
            }
            else
            {
                // Get the MonitorInfoData for the monitor where the handled window will be placed after it unregisters
                // as an AppBar. In case the monitor the window came from before it registered does still exist, that
                // monitor will be returned. Otherwise the currentMonitor will default to the primary monitor.
                WinApiRectangle windowRectangle = new WinApiRectangle();
                windowRectangle.left = (int)_originalWindowAttributes.windowLeft;
                windowRectangle.top = (int)_originalWindowAttributes.windowTop;
                windowRectangle.right = (int)(_originalWindowAttributes.windowLeft + _originalWindowAttributes.windowWidth);
                windowRectangle.bottom = (int)(_originalWindowAttributes.windowTop + _originalWindowAttributes.windowHeight);
                MonitorInfoData currentMonitor = HandleGetMonitorInfoFromRect(windowRectangle);

                // Conversion variables needed to put the window on whichever monitor it will be put on at the same position
                // it was at on the monitor it was on before it registered as an AppBar.
                int currentMonitorHeight = currentMonitor.rcMonitor.bottom - currentMonitor.rcMonitor.top;
                int currentMonitorWidth = currentMonitor.rcMonitor.right - currentMonitor.rcMonitor.left;
                double monitorHeightFactor = (double)currentMonitorHeight / _originalWindowAttributes.monitorHeight;
                double monitorWidthFactor = (double)currentMonitorWidth / _originalWindowAttributes.monitorWidth;
                double windowTopRelative = (double)((int)_originalWindowAttributes.windowTop - _originalWindowAttributes.monitorTop) / _originalWindowAttributes.monitorHeight;
                double windowLeftRelative = (double)((int)_originalWindowAttributes.windowLeft - _originalWindowAttributes.monitorLeft) / _originalWindowAttributes.monitorWidth;

                _windowToHandle.Top = currentMonitor.rcMonitor.top + windowTopRelative * currentMonitorHeight;
                _windowToHandle.Height = _originalWindowAttributes.windowHeight * monitorHeightFactor;
                _windowToHandle.Left = currentMonitor.rcMonitor.left + windowLeftRelative * currentMonitorWidth;
                _windowToHandle.Width = _originalWindowAttributes.windowWidth * monitorWidthFactor;

                _windowToHandle.Topmost = _originalWindowAttributes.topMost;
                _windowToHandle.WindowStyle = _originalWindowAttributes.windowStyle;
                _windowToHandle.ResizeMode = _originalWindowAttributes.resizeMode;
                _windowToHandle.WindowState = _originalWindowAttributes.windowState;
            }
        }

        // Sets the initial values for desiredRectangle and appBarData.rc dpending on _currentAppBarAttributes.screenEdge and
        // _currentAppBarAttributes.margin.
        private void SetInitialRectangle(ref AppBarData appBarData, ref WinApiRectangle desiredRectangle, MonitorInfoData currentMonitor)
        {
            // Used in conjunction with _currentAppBarAttributes.margin to work out either the height or the width of the AppBar in pixels.
            int currentMonitorHeight = currentMonitor.rcMonitor.bottom - currentMonitor.rcMonitor.top;
            int currentMonitorWidth = currentMonitor.rcMonitor.right - currentMonitor.rcMonitor.left;

            // Check which screen to bound the AppBar to and then set the rectangle values.
            if (_currentAppBarAttributes.screenEdge == ScreenEdge.Left || _currentAppBarAttributes.screenEdge == ScreenEdge.Right)
            {
                desiredRectangle.top = currentMonitor.rcMonitor.top;
                desiredRectangle.bottom = currentMonitor.rcMonitor.bottom;
                appBarData.rc.top = desiredRectangle.top;
                appBarData.rc.bottom = desiredRectangle.bottom;
                if (_currentAppBarAttributes.screenEdge == ScreenEdge.Left)
                {
                    desiredRectangle.left = currentMonitor.rcMonitor.left;
                    desiredRectangle.right = currentMonitor.rcMonitor.left + (int)(currentMonitorWidth * _currentAppBarAttributes.visibleMargin);
                    appBarData.rc.left = desiredRectangle.left;
                    appBarData.rc.right = desiredRectangle.right;
                }
                else
                {
                    desiredRectangle.right = currentMonitor.rcMonitor.right;
                    desiredRectangle.left = currentMonitor.rcMonitor.right - (int)(currentMonitorWidth * _currentAppBarAttributes.visibleMargin);
                    appBarData.rc.right = desiredRectangle.right;
                    appBarData.rc.left = desiredRectangle.left;
                }
            }
            else
            {
                desiredRectangle.left = currentMonitor.rcMonitor.left;
                desiredRectangle.right = currentMonitor.rcMonitor.right;
                appBarData.rc.left = desiredRectangle.left;
                appBarData.rc.right = desiredRectangle.right;
                if (_currentAppBarAttributes.screenEdge == ScreenEdge.Top)
                {
                    desiredRectangle.top = currentMonitor.rcMonitor.top;
                    desiredRectangle.bottom = currentMonitor.rcMonitor.top + (int)(currentMonitorHeight * _currentAppBarAttributes.visibleMargin);
                    appBarData.rc.top = desiredRectangle.top;
                    appBarData.rc.bottom = desiredRectangle.bottom;
                }
                else
                {
                    desiredRectangle.bottom = currentMonitor.rcMonitor.bottom;
                    desiredRectangle.top = currentMonitor.rcMonitor.bottom - (int)(currentMonitorHeight * _currentAppBarAttributes.visibleMargin);
                    appBarData.rc.bottom = desiredRectangle.bottom;
                    appBarData.rc.top = desiredRectangle.top;
                }
            }
        }
        
        // After the ABM_QUERYPOS message has been send to the operating system, this method tries to adjust for any changes made by the system
        // to the AppBarData.rc rectangle. An example for why this is needed: If the AppBar is supposed to go to the bottom of the screen and
        // the taskbar is located there, the system will adjust the desired AppBar position so that it doesnt overlap the taskbar. It will do this
        // by setting the bottom of the desired rectangle to the taskbar top. And leave the desired top position of the AppBar rectangle unchanged
        // (if it doesnt overlap the taskbar as well, in that case it will be set to the taskbar top as well). This method tries to adjust this
        // change so that the width or height (depending on the defined screen edge) value of the AppBar is unchanged by the system correction. 
        private void AdjustQueriedAppBarDataRectangle(ref AppBarData appBarData, WinApiRectangle desiredRectangle, MonitorInfoData currentMonitor)
        {
            ScreenEdge screenEdge = (ScreenEdge)appBarData.uEdge;

            int desiredHeight = desiredRectangle.bottom - desiredRectangle.top;
            int desiredWidth = desiredRectangle.right - desiredRectangle.left;

            switch (screenEdge)
            {
                case ScreenEdge.Left:
                    appBarData.rc.right = appBarData.rc.left + desiredWidth;
                    break;
                case ScreenEdge.Top:
                    appBarData.rc.bottom = appBarData.rc.top + desiredHeight;
                    break;
                case ScreenEdge.Right:
                    appBarData.rc.left = appBarData.rc.right - desiredWidth;
                    break;
                case ScreenEdge.Bottom:
                    appBarData.rc.top = appBarData.rc.bottom - desiredHeight;
                    break;
            }
        }

        // Checks if the AppBarData.rc rectangle that was set by the system when it received the ABM_SETPOS message
        // is equal in width or height (depending on the defined screen edge) to the desired rectangle and does not
        // violate the boundaries of the current monitor. This method is here to guarantee that only those AppBars 
        // are placed, that do not have unwanted width or height values because the available screen space was not
        // enough.
        private bool AppBarDataRectangleIsDesired(AppBarData appBarData, WinApiRectangle desiredRectangle, MonitorInfoData currentMonitor)
        {
            ScreenEdge screenEdge = (ScreenEdge)appBarData.uEdge;

            int desiredHeight = desiredRectangle.bottom - desiredRectangle.top;
            int desiredWidth = desiredRectangle.right - desiredRectangle.left;
            int actualHeight = appBarData.rc.bottom - appBarData.rc.top;
            int actualWidth = appBarData.rc.right - appBarData.rc.left;

            if (screenEdge == ScreenEdge.Left || screenEdge == ScreenEdge.Right)
            {
                if (desiredWidth != actualWidth)
                {
                    return false;
                }
                if (screenEdge == ScreenEdge.Left)
                {
                    if (appBarData.rc.right > currentMonitor.rcMonitor.right)
                    {
                        return false;
                    }
                }
                else
                {
                    if (appBarData.rc.left < currentMonitor.rcMonitor.left)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (desiredHeight != actualHeight)
                {
                    return false;
                }
                if (screenEdge == ScreenEdge.Top)
                {
                    if (appBarData.rc.bottom > currentMonitor.rcMonitor.bottom)
                    {
                        return false;
                    }
                }
                else
                {
                    if (appBarData.rc.top < currentMonitor.rcMonitor.top)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        #endregion


        #region External Functions (unmanaged code)
        // Sends an AppBar message to the operating system (i.e. does all the actual AppBar stuff, like registering and removing it).
        // Useful side note: The microsoft docs state that for some messages (e.g. ABM_REMOVE) this method always returns true (1).
        // However if the C# implementation is incorrect (e.g. wrong types in AppBarData) the function will return false (0) instead.
        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
        private static extern uint SHAppBarMessage(int dwMessage, ref AppBarData pData);

        // Registers a message value with the operating system, that is guaranteed to be unique throughout the system for a given 'msg' string.
        // This function is needed in order for the AppBar to be able to receive notifications from the operating system.
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern uint RegisterWindowMessage(string msg);

        // Retrieves information about a display monitor and stores that information in a 'MonitorInfoData' struct.
        // This function is needed to place the AppBar properly on multiple display setups.
        [DllImport("User32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoData lpmi);

        // Retrieves a handle to the display monitor that has the largest area of intersection with a specified rectangle.
        // This function is used to get the monitor handle for the 'GetMonitorInfo' function.
        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromRect(ref WinApiRectangle lprc, int dwFlags);
        #endregion
    }
}
