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
        // To receive notifications from the OS, the AppBar needs to provide a CallbackID.
        private int _callbackID;
        // The window which the AppBarHandler is handling.
        private Window _windowToHandle;
        // The attributes of the _windowToHandle before it became and AppBar.
        private WPFWindowAttributes _originalWindowAttributes;
        // The attributes of the AppBar. This should always represent the current attributes.
        private AppBarAttributes _currentAppBarAttributes;
        #endregion


        #region Properties
        // Encapsulates _appBarType.
        public bool AppBarIsAutoHide
        {
            get { return _currentAppBarAttributes.isAutoHide; }
            // Ideally the AppBarType can be changed by the caller at any time.
            set { throw new NotImplementedException(); }
        }
        // Encapsulates _callbackID.
        public int CallbackID
        {
            get { return _callbackID; }
        }
        // Encapsulates _currentAppBarAttributes.isRegistered.
        public bool AppBarIsRegistered
        {
            get { return _currentAppBarAttributes.isRegistered; }
        }
        #endregion


        #region Constructors
        public AppBarHandler(Window windowToHandle)
        {
            // Sets the CallbackID field to a unique value based on the datetime when the handler is initialized.
            _callbackID = (int)RegisterWindowMessage(String.Format("AppBarHandler_{0}", DateTime.Now.Ticks));
            // -
            _windowToHandle = windowToHandle;
        }
        #endregion


        #region Public Methods
        // Registers the WindowToHandle as an AppBar and places it to the specified position.
        public bool PlaceAppBar(bool isAutoHide, ScreenEdge screenEdge, double margin)
        {
            if (!_currentAppBarAttributes.isRegistered)
            {
                _currentAppBarAttributes.isAutoHide = isAutoHide;

                if (!_currentAppBarAttributes.isAutoHide)
                {
                    if (HandleAppBarNew() == true)
                    {
                        // Save all of the attributes needed to restore the window to its original position once
                        // it unregisters as an AppBar.
                        SaveRestoreOriginalWindowAttributes(doSave: true);

                        // The AppBar should have no borders at all.
                        _windowToHandle.WindowStyle = WindowStyle.None;
                        _windowToHandle.ResizeMode = ResizeMode.NoResize;
                        // Trying to move the _windowToHandle while it is in the maximized state doesn't work.
                        _windowToHandle.WindowState = WindowState.Normal;

                        _currentAppBarAttributes.screenEdge = screenEdge;
                        _currentAppBarAttributes.margin = margin;

                        HandleAppBarQueryPosSetPos();
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            // *** There could and maybe should be some more code here: ***
            // Event to indicate that the AppBar has been placed.
            // ************************************************************

            return true;
        }

        // Moves _windowToHandle if it is already an AppBar
        public bool MoveAppBar(ScreenEdge screenEdge, double margin)
        {
            if (_currentAppBarAttributes.isRegistered)
            {
                _currentAppBarAttributes.screenEdge = screenEdge;
                _currentAppBarAttributes.margin = margin;

                HandleAppBarQueryPosSetPos();
            }
            else
            {
                return false;
            }

            return true;
        }

        // Removes the AppBar and sets the WindowToHandle back to its position before it became an AppBar.
        public bool RemoveAppBar()
        {
            if (_currentAppBarAttributes.isRegistered)
            {
                HandleAppBarRemove();
                SaveRestoreOriginalWindowAttributes(doSave: false);
            }

            // *** There could and maybe should be some more code here: ***
            // Event to indicate that the AppBar has been removed.
            // ************************************************************

            return true;
        }
        #endregion


        #region Methods to encapsulate the interaction with the operating system
        // Testing ...
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
            // Setting up the HwndSource object (Win32 representation of a WPF window) based on the _windowToHandle field.
            WindowInteropHelper windowHelper = new WindowInteropHelper(_windowToHandle);
            HwndSource windowSource = HwndSource.FromHwnd(windowHelper.Handle);

            // Specifying the AppBarData to be supplied to the SHAppBarMessage function.
            AppBarData appBarData = new AppBarData();
            appBarData.cbSize = Marshal.SizeOf(appBarData);
            appBarData.hWnd = windowSource.Handle;
            appBarData.uCallbackMessage = _callbackID;

            // Call the SHAppBarMessage function and let it do its magic. If it fails return false.
            if (SHAppBarMessage((int)MessageIdentifier.ABM_NEW, ref appBarData) == 0)
            {
                return false;
            }

            // Since the AppBar is now registered, it must receive certain notifications and handle them via WindowProc.
            // Therefore a hook is added to the HwndSource object of the WPF window.
            windowSource.AddHook(new HwndSourceHook(ProcessWinApiMessages));

            _currentAppBarAttributes.isRegistered = true;
            return true;
        }

        // Works out the position of the AppBar and reserves the space (i.e. calls SHAppBarData with the MessageIdentifiers ABM_QUERYPOS and ABM_SETPOS).
        private void HandleAppBarQueryPosSetPos()
        {
            // Setting up the HwndSource object (Win32 representation of a WPF window) based on the _windowToHandle field.
            WindowInteropHelper windowHelper = new WindowInteropHelper(_windowToHandle);
            HwndSource windowSource = HwndSource.FromHwnd(windowHelper.Handle);

            // Specifying the AppBarData to be supplied to the SHAppBarMessage function.
            AppBarData appBarData = new AppBarData();
            appBarData.cbSize = Marshal.SizeOf(appBarData);
            appBarData.hWnd = windowSource.Handle;
            appBarData.uEdge = (int)_currentAppBarAttributes.screenEdge;

            // ...
            WinApiRectangle windowRectangle = new WinApiRectangle();
            windowRectangle.left = (int)_windowToHandle.Left;
            windowRectangle.top = (int)_windowToHandle.Top;
            windowRectangle.right = (int)(_windowToHandle.Left + _windowToHandle.Width);
            windowRectangle.bottom = (int)(_windowToHandle.Top + _windowToHandle.Height);
            MonitorInfoData currentMonitor = HandleGetMonitorInfoFromRect(windowRectangle);

            int currentMonitorHeight = currentMonitor.rcMonitor.bottom - currentMonitor.rcMonitor.top;
            int currentMonitorWidth = currentMonitor.rcMonitor.right - currentMonitor.rcMonitor.left;

            // Specify the dimensions of the AppBar based on the '_currentAppBarAttributes.screenEdge' and '_currentAppBarAttributes.margin' values.
            if (_currentAppBarAttributes.screenEdge == ScreenEdge.Left || _currentAppBarAttributes.screenEdge == ScreenEdge.Right)
            {
                appBarData.rc.top = currentMonitor.rcMonitor.top;
                appBarData.rc.bottom = currentMonitor.rcMonitor.bottom;
                if (_currentAppBarAttributes.screenEdge == ScreenEdge.Left)
                {
                    appBarData.rc.left = currentMonitor.rcMonitor.left;
                    appBarData.rc.right = currentMonitor.rcMonitor.left + (int)(currentMonitorWidth * _currentAppBarAttributes.margin);
                }
                else
                {
                    appBarData.rc.right = currentMonitor.rcMonitor.right;
                    appBarData.rc.left = currentMonitor.rcMonitor.right - (int)(currentMonitorWidth * _currentAppBarAttributes.margin);
                }
            }
            else
            {
                appBarData.rc.left = currentMonitor.rcMonitor.left;
                appBarData.rc.right = currentMonitor.rcMonitor.right;
                if (_currentAppBarAttributes.screenEdge == ScreenEdge.Top)
                {
                    appBarData.rc.top = currentMonitor.rcMonitor.top;
                    appBarData.rc.bottom = currentMonitor.rcMonitor.top + (int)(currentMonitorHeight * _currentAppBarAttributes.margin);
                }
                else
                {
                    appBarData.rc.bottom = currentMonitor.rcMonitor.bottom;
                    appBarData.rc.top = currentMonitor.rcMonitor.bottom - (int)(currentMonitorHeight * _currentAppBarAttributes.margin);
                }
            }

            // Call the SHAppBarMessage function to first query the position where the AppBar should go an then based on the updated
            // appBarData parameter set the position (reserve it).
            uint testTrue;
            testTrue = SHAppBarMessage((int)MessageIdentifier.ABM_QUERYPOS, ref appBarData);
            testTrue = SHAppBarMessage((int)MessageIdentifier.ABM_SETPOS, ref appBarData);

            // Move and size the AppBar to fit the bounding rectangle passed to the operating system by the last call to the 
            // SHAppBarMessage function. I guess this wont work because the space I want to move to is reserved?.
            _windowToHandle.Top = appBarData.rc.top;
            _windowToHandle.Left = appBarData.rc.left;
            _windowToHandle.Width = appBarData.rc.right - appBarData.rc.left;
            _windowToHandle.Height = appBarData.rc.bottom - appBarData.rc.top;
        }

        // Removes the AppBar from the operating system.
        private void HandleAppBarRemove()
        {
            // Setting up the HwndSource object (Win32 representation of a WPF window) based on the _windowToHandle field.
            WindowInteropHelper windowHelper = new WindowInteropHelper(_windowToHandle);
            HwndSource windowSource = HwndSource.FromHwnd(windowHelper.Handle);

            // Specifying the AppBarData to be supplied to the SHAppBarMessage function.
            AppBarData appBarData = new AppBarData();
            appBarData.cbSize = Marshal.SizeOf(appBarData);
            appBarData.hWnd = windowSource.Handle;

            // Call the SHAppBarMessage function to remove the AppBar from the operating system.
            SHAppBarMessage((int)MessageIdentifier.ABM_REMOVE, ref appBarData);

            // Since the AppBar is not registered any longer, no messages from the operating system should receive special treatment anymore.
            // Therefore the hook is removed from the HwndSource object of the WPF window.
            windowSource.RemoveHook(new HwndSourceHook(ProcessWinApiMessages));

            _currentAppBarAttributes.isRegistered = false;
        }

        // Processes window messages send by the operating system. This is a callback function that requires a hook on the
        // Win32 representation of the _windowToHandle (HwndSource object). This hook is added upon registration of the AppBar
        // and removed upon removal of the AppBar. This is the implementation of the placeholder function named WindowProc
        // (use that name to look it up in the microsoft docs).

        private int wtfWhy;
        
        private IntPtr ProcessWinApiMessages(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (uMsg == _callbackID)
            {
                if (wParam.ToInt32() == (int)NotificationIdentifier.ABN_POSCHANGED)
                {
                    HandleAppBarQueryPosSetPos();
                    handled = true;

                    wtfWhy += 1;
                    System.Diagnostics.Debug.Print("ABN_POSCHANGED {0}", wtfWhy);
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

                _windowToHandle.WindowStyle = _originalWindowAttributes.windowStyle;
                _windowToHandle.ResizeMode = _originalWindowAttributes.resizeMode;
                _windowToHandle.WindowState = _originalWindowAttributes.windowState;
            }
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
