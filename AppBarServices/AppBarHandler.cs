// The class 'AppBarHandler' implements all of the logic of 'AppBarServices'. It is the only class defined with the public modifier
// and its intended use is for a WPF window to define it as a member and call its public methods in order to implement an AppBar. 

using System.Diagnostics;

using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;

using AppBarServices.Enums;
using AppBarServices.Structs;

namespace AppBarServices
{
    /// <summary>
    /// Implements all of the logic of the AppBarServices library. It is the only class defined with the public modifier
    /// and its intended use is for a WPF window to define it as a member and call its public methods in order to implement an AppBar.
    /// </summary>
    public class AppBarHandler
    {
        #region Fields
        /// <summary>
        /// The window which the AppBarHandler is handling.
        /// </summary>
        private Window _windowToHandle;
        /// <summary>
        /// Represents the window to handle as a WinApi window. Used for WinApi interoperability.
        /// </summary>
        private HwndSource _windowSource;
        /// <summary>
        /// To receive notifications from the OS, the AppBar window needs to provide a CallbackID.
        /// </summary>
        private int _callbackID;
        /// <summary>
        /// The attributes of the handled window before it became an AppBar. Used to restore the window back to its original
        /// form, when the AppBar is removed.
        /// </summary>
        private WPFWindowAttributes _originalWindowAttributes;
        /// <summary>
        /// The AppBar attributes as they are at any given point in the liefetime of the AppBar. These attributes should only
        /// be set by methods of the AppBarHandler.
        /// </summary>
        private CurrentAppBarAttributes _currentAppBarAttributes;
        /// <summary>
        /// The desired AppBar attrbuts as supplied by the library user.
        /// </summary>
        private DesiredAppBarAttributes _desiredAppBarAttributes;
        /// <summary>
        /// The attributes related to the preview capabilities of the AppBarHandler.
        /// </summary>
        private PreviewAttributes _previewAttributes;
        #endregion


        #region Properties
        /// <summary>
        /// Indicates whether the AppBar is currently registered or not.
        /// </summary>
        public bool AppBarIsRegistered
        {
            get { return _currentAppBarAttributes.isRegistered; }
        }

        // Gets or sets _currentAppBarAttributes.isRegisteredAutoHide.
        public bool AppBarIsRegisteredAutoHide
        {
            get { return _currentAppBarAttributes.isRegisteredAutoHide; }
            set
            {
                throw new NotImplementedException();
            }
        }
        
        /// <summary>
        /// Gets or sets the margin of the AppBar that is used when it is not hidden.
        /// </summary>
        public double AppBarVisibleMargin
        {
            get { return _currentAppBarAttributes.visibleMargin; }
            set { throw new NotImplementedException(); }
        }
        
        /// <summary>
        /// Gets or sets the margin of the AppBar that is used when it is hidden. This value is only used by AutoHide AppBars.
        /// </summary>
        public double AppBarHiddenMargin
        {
            get { return _currentAppBarAttributes.hiddenMargin; }
            set { throw new NotImplementedException(); }
        }
        
        /// <summary>
        /// Gets the value of the callbackID for communication with the operating system.
        /// </summary>
        public int WindowCallbackID
        {
            get { return _callbackID; }
        }
        #endregion


        #region Constructors
        /// <summary>
        /// Initializes the AppBar. Note: The initialization of the AppBarHandler does not lead to a placed AppBar. This is either done by
        /// calling AppBarHandler.PlaceAppBar or by using the PreviewToSnap functionality of the AppBarHandler.
        /// </summary>
        /// <param name="windowToHandle">The window object that should be handled.</param>
        /// <param name="desiredAppBarAttributes">Defines how the AppBar should look and behave once it gets placed.</param>
        /// <param name="previewAttributes">Defines if the PreviewToSnap functionality of the AppBarHandler should be enabled and provides 
        /// some customization options for said functionality.</param>
        public AppBarHandler(Window windowToHandle, DesiredAppBarAttributes desiredAppBarAttributes, PreviewAttributes previewAttributes)
        {
            // Sets the CallbackID field to a unique value based on the datetime when the handler is initialized.
            _callbackID = (int)RegisterWindowMessage(String.Format("AppBarHandler_{0}", DateTime.Now.Ticks));
            
            // Set the _windowToHandle field.
            _windowToHandle = windowToHandle;

            // Set the _windowSource using the _windowToHandle.
            WindowInteropHelper windowHelper = new WindowInteropHelper(_windowToHandle);
            _windowSource = HwndSource.FromHwnd(windowHelper.Handle);

            // Add the hook for the ProcessWinApiMessage function (implementation of the WindowProc placeholder) to process
            // operating system messages.
            _windowSource.AddHook(new HwndSourceHook(ProcessWinApiMessages));
            _windowToHandle.Closed += HandledWindowClosed;

            // Set the _desiredAppBarAttributes and _previewAttributes.
            _desiredAppBarAttributes = desiredAppBarAttributes;
            _previewAttributes = previewAttributes;
        }

        private void HandledWindowClosed(object sender, EventArgs e)
        {
            _windowSource.RemoveHook(new HwndSourceHook(ProcessWinApiMessages));
        }
        #endregion


        #region Public Methods
        // Registers the WindowToHandle as an AppBar and places it to the specified position.
        public bool PlaceAppBar(bool isAutoHide, ScreenEdge screenEdge, double visibleMargin, double hiddenMargin = 0.01)
        {
            if (!_currentAppBarAttributes.isRegistered)
            {
                // Setting up the HwndSource object (Win32 representation of a WPF window) based on the _windowToHandle field.
                // Important note: This can only be done once the _windowToHandle was initialized (after its constructor ran).
                // Therefore I did not put it in the constructor of the AppBarHandler, which means that at least the 
                // initialization of the AppBarHandler can take place in the constructor of the handled window.
                WindowInteropHelper windowHelper = new WindowInteropHelper(_windowToHandle);
                _windowSource = HwndSource.FromHwnd(windowHelper.Handle);

                if (SendAppBarNew())
                {
                    // Save all of the attributes needed to restore the window to its original position once
                    // it unregisters as an AppBar.
                    SaveRestoreOriginalWindowAttributes(doSave: true);
                    
                    _currentAppBarAttributes.screenEdge = screenEdge;
                    _currentAppBarAttributes.visibleMargin = visibleMargin;
                    _currentAppBarAttributes.hiddenMargin = hiddenMargin;

                    if (!isAutoHide)
                    {
                        if (!SendAppBarQueryPosSetPos(doHide: false))
                        {
                            RemoveAppBar();
                            return false;
                        }
                    }
                    else
                    {
                        if (SendSetAutoHideBarEx(doRegister: true))
                        {
                            if (!SendAppBarQueryPosSetPos(doHide: true))
                            {
                                RemoveAppBar();
                                return false;
                            }
                        }
                        else
                        {
                            RemoveAppBar();
                            return false;
                        }
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

        // Moves _windowToHandle to the defined screenEdge if it already is an AppBar.
        public bool MoveAppBar(ScreenEdge screenEdge)
        {
            if (_currentAppBarAttributes.isRegistered)
            {
                if (_currentAppBarAttributes.isRegisteredAutoHide)
                {
                    SendSetAutoHideBarEx(doRegister: false);

                    _currentAppBarAttributes.screenEdge = screenEdge;

                    if (!SendSetAutoHideBarEx(doRegister: true))
                    {
                        RemoveAppBar();
                        return false;
                    }
                }

                _currentAppBarAttributes.screenEdge = screenEdge;

                if (!SendAppBarQueryPosSetPos(doHide: _currentAppBarAttributes.isRegisteredAutoHide))
                {
                    RemoveAppBar();
                    return false;
                }
            }

            return true;
        }

        // Removes the AppBar and sets the handled window back to the position it hade before it became an AppBar.
        public void RemoveAppBar()
        {
            if (_currentAppBarAttributes.isRegistered)
            {
                // First check if the AppBar is registered as AutoHide and if so unregister. 
                if (_currentAppBarAttributes.isRegisteredAutoHide)
                {
                    SendSetAutoHideBarEx(doRegister: false);
                }

                // Then unregister the AppBar.
                SendAppBarRemove();

                // Finally restore the original window attributes.
                SaveRestoreOriginalWindowAttributes(doSave: false);
            }
        }
        #endregion


        #region Methods to encapsulate the interaction with the operating system
        /// <summary>
        /// Gets information in the form of a MonitorInfoData struct of the monitor that contains the point specified by the point parameter.
        /// If the specified point is not contained in any monitor, the method defaults to the primary monitor.
        /// </summary>
        /// <param name="point">The point for which the containing monitors MonitorInfoData should be retrieved.</param>
        /// <returns>Returns a MonitorInfoData struct.</returns>
        private MonitorInfoData HandleGetMonitorInfoFromPoint(WinApiPoint point)
        {
            MonitorInfoData monitorInfoData = new MonitorInfoData();
            monitorInfoData.cbSize = Marshal.SizeOf(monitorInfoData);

            IntPtr monitorHandle;
            monitorHandle = MonitorFromPoint(point, (int)MonitorFromRectOnNoIntersection.MONITOR_DEFAULTTOPRIMARY);
            GetMonitorInfo(monitorHandle, ref monitorInfoData);

            return monitorInfoData;
        }

        /// <summary>
        /// Gets information in the form of a MonitorInfoData struct of the monitor that that has the largest area of intersection with
        /// the provided rectangle parameter. If the specified rectangle has no intersaection with any monitor, the method defaults to 
        /// the primary monitor.
        /// </summary>
        /// <param name="rectangle">The rectangle for which the MonitorInfoData for the monitor with the largest area of intersection
        /// should be retrieved.</param>
        /// <returns>Returns a MonitorInfoData struct.</returns>
        internal MonitorInfoData HandleGetMonitorInfoFromRect(WinApiRectanglePoints rectangle)
        {
            MonitorInfoData monitorInfoData = new MonitorInfoData();
            monitorInfoData.cbSize = Marshal.SizeOf(monitorInfoData);

            IntPtr monitorHandle;
            monitorHandle = MonitorFromRect(ref rectangle, (int)MonitorFromRectOnNoIntersection.MONITOR_DEFAULTTOPRIMARY);
            GetMonitorInfo(monitorHandle, ref monitorInfoData);

            return monitorInfoData;
        }

        /// <summary>
        /// Gets information in the form of a MonitorInfoData struct of the monitor the handled window is currently on.
        /// If the handled window does not overlap with any monitor, the method defaults to the primary monitor.
        /// </summary>
        /// <returns>Returns a MonitorInfoData struct.</returns>
        internal MonitorInfoData HandleGetMonitorInfoFromWindowHandle()
        {
            MonitorInfoData monitorInfoData = new MonitorInfoData();
            monitorInfoData.cbSize = Marshal.SizeOf(monitorInfoData);

            IntPtr monitorHandle;
            monitorHandle = MonitorFromWindow(_windowSource.Handle, (int)MonitorFromRectOnNoIntersection.MONITOR_DEFAULTTOPRIMARY);
            GetMonitorInfo(monitorHandle, ref monitorInfoData);

            return monitorInfoData;
        }
        
        /// <summary>
        /// Registers the AppBar with the operating system by sending the ABM_NEW message via SHAppBarData.
        /// Before finishing it also sets _currentAppBarAttributs.isRegistered to true.
        /// </summary>
        /// <returns>Returns true if the registration was successful, false otherwise.</returns>
        private bool SendAppBarNew()
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
        
        /// <summary>
        /// Works out the position of the AppBar, reserves the space (i.e. calls SHAppBarData with the MessageIdentifiers ABM_QUERYPOS and ABM_SETPOS)
        /// and moves the WPF window to the reserved space if that space has the margin expected (i.e. defined) by the caller.
        /// </summary>
        /// <returns>Returns true if successful, false otherwise</returns>
        private bool SendAppBarQueryPosSetPos(MonitorInfoData monitorToPlaceOn)
        {
            // Specifying the AppBarData to be supplied to the SHAppBarMessage function.
            AppBarData appBarData = new AppBarData();
            appBarData.cbSize = Marshal.SizeOf(appBarData);
            appBarData.hWnd = _windowSource.Handle;
            appBarData.uEdge = (int)_desiredAppBarAttributes.screenEdge;

            // The rectangle that the caller wants the AppBar to have. This variable is used to check whether the AppBar rectangle set by the
            // operating system is the same width- or height-wise (depending on the defined screen edge) as the rectangle desired by the caller.
            // Only if that is the case will the AppBar be set in the end.
            WinApiRectanglePoints desiredRectangle = new WinApiRectanglePoints();

            // Specify the dimensions of the AppBar based on the '_currentAppBarAttributes.screenEdge' and '_currentAppBarAttributes.margin' values.
            SetInitialRectangle(ref appBarData, ref desiredRectangle, monitorToPlaceOn, _desiredAppBarAttributes.doAutoHide);

            // Query the position where the AppBar should go and let the operating system adjust it for any obstacles.
            // Then take the adjusted values and correct them to represent the desired width or height value
            // (depending on the defined screen edge).
            SHAppBarMessage((int)MessageIdentifier.ABM_QUERYPOS, ref appBarData);
            AdjustQueriedAppBarDataRectangle(ref appBarData, desiredRectangle, monitorToPlaceOn);

            // Reserve the position for the AppBar and then check if the position set by the operating system is equal in
            // width or height (depending on the defined screen edge) to the desired width or height value. If that is not
            // the case return false.
            uint testReturn;
            testReturn = SHAppBarMessage((int)MessageIdentifier.ABM_SETPOS, ref appBarData);
            if (AppBarDataRectangleIsDesired(appBarData, desiredRectangle, monitorToPlaceOn) == false)
            {
                _currentAppBarAttributes.isHidden = false;
                return false;
            }

            // Move the WPF window to the reserved position. Do this using the MoveWindow function from the WinApi, because this looks smother
            // than doing it in WPF.
            int X = appBarData.rc.left;
            int Y = appBarData.rc.top;
            int nWidth = appBarData.rc.right - appBarData.rc.left;
            int nHeight = appBarData.rc.bottom - appBarData.rc.top;
            MoveWindow(_windowSource.Handle, X, Y, nWidth, nHeight, true);

            // Update the _curremtAppBarAttributes (windowRectangle)
            _currentAppBarAttributes.windowRectangle.left = appBarData.rc.left;
            _currentAppBarAttributes.windowRectangle.top = appBarData.rc.top;
            _currentAppBarAttributes.windowRectangle.right = appBarData.rc.right;
            _currentAppBarAttributes.windowRectangle.bottom = appBarData.rc.bottom;
            // Update the _curremtAppBarAttributes (screenRectangle)
            _currentAppBarAttributes.screenRectangle.left = monitorToPlaceOn.rcMonitor.left;
            _currentAppBarAttributes.screenRectangle.top = monitorToPlaceOn.rcMonitor.top;
            _currentAppBarAttributes.screenRectangle.right = monitorToPlaceOn.rcMonitor.right;
            _currentAppBarAttributes.screenRectangle.bottom = monitorToPlaceOn.rcMonitor.bottom;
            // Update the _curremtAppBarAttributes (screenEdge)
            _currentAppBarAttributes.screenEdge = _desiredAppBarAttributes.screenEdge;
            // Update the _curremtAppBarAttributes (isHidden)
            _currentAppBarAttributes.isHidden = _desiredAppBarAttributes.doAutoHide;

            // Everything went well, so return true.
            return true;
        }

        /// <summary>
        /// Unregisters the AppBar from the operating system by sending the ABM_REMOVE message via SHAppBarMessage.
        /// Before finishing it also sets _currentAppBarAttributs.isRegistered to false.
        /// </summary>
        private void SendAppBarRemove()
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

        // Registers or unregisters an AppBar as AutoHide (i.e. calls SHAppBarData with the MessageIdentifier ABM_SETAUTOHIDEBAREX).
        private bool SendSetAutoHideBarEx(MonitorInfoData monitorToSetOn)
        {
            // Specifying the AppBarData to be supplied to the SHAppBarMessage function part 1.
            AppBarData appBarData = new AppBarData();
            appBarData.cbSize = Marshal.SizeOf(appBarData);
            appBarData.lParam = Convert.ToInt32(_desiredAppBarAttributes.doAutoHide);
            appBarData.uEdge = (int)_desiredAppBarAttributes.screenEdge;
            appBarData.hWnd = _windowSource.Handle;

            // Specifying the AppBarData to be supplied to the SHAppBarMessage function part 2.
            appBarData.rc.left = monitorToSetOn.rcMonitor.left;
            appBarData.rc.top = monitorToSetOn.rcMonitor.top;
            appBarData.rc.right = monitorToSetOn.rcMonitor.right;
            appBarData.rc.bottom = monitorToSetOn.rcMonitor.bottom;

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

                _windowToHandle.MouseEnter += HandledWindowMouseEnter;
                _windowToHandle.MouseLeave += HandledWindowMouseLeave;
            }
            else
            {
                _currentAppBarAttributes.isRegisteredAutoHide = false;

                _windowToHandle.MouseEnter -= HandledWindowMouseEnter;
                _windowToHandle.MouseLeave -= HandledWindowMouseLeave;
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
                    SendAppBarQueryPosSetPos(doHide: _currentAppBarAttributes.isHidden);
                    handled = true;
                }
            }
            if (uMsg == 534)
            {
                System.Diagnostics.Debug.Print("HalliHallo");
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
                
                MonitorInfoData originalMonitor = HandleGetMonitorInfoFromWindowHandle();
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
                WinApiRectanglePoints windowRectangle = new WinApiRectanglePoints();
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

        // Sets the initial values for desiredRectangle and appBarData.rc depending on _currentAppBarAttributes.screenEdge and
        // _currentAppBarAttributes.margin.
        private void SetInitialRectangle(ref AppBarData appBarData, ref WinApiRectanglePoints desiredRectangle, MonitorInfoData currentMonitor, bool doHide)
        {
            // Used in conjunction with _currentAppBarAttributes.margin to work out either the height or the width of the AppBar in pixels.
            int currentMonitorHeight = currentMonitor.rcMonitor.bottom - currentMonitor.rcMonitor.top;
            int currentMonitorWidth = currentMonitor.rcMonitor.right - currentMonitor.rcMonitor.left;

            // Determine, whether the AppBar should be treated as hidden or not and set the margin to be used accordingly.
            double margin;
            if (doHide)
            {
                margin = _currentAppBarAttributes.hiddenMargin;
            }
            else
            {
                margin = _currentAppBarAttributes.visibleMargin;
            }

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
                    desiredRectangle.right = currentMonitor.rcMonitor.left + (int)(currentMonitorWidth * margin);
                    appBarData.rc.left = desiredRectangle.left;
                    appBarData.rc.right = desiredRectangle.right;
                }
                else
                {
                    desiredRectangle.right = currentMonitor.rcMonitor.right;
                    desiredRectangle.left = currentMonitor.rcMonitor.right - (int)(currentMonitorWidth * margin);
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
                    desiredRectangle.bottom = currentMonitor.rcMonitor.top + (int)(currentMonitorHeight * margin);
                    appBarData.rc.top = desiredRectangle.top;
                    appBarData.rc.bottom = desiredRectangle.bottom;
                }
                else
                {
                    desiredRectangle.bottom = currentMonitor.rcMonitor.bottom;
                    desiredRectangle.top = currentMonitor.rcMonitor.bottom - (int)(currentMonitorHeight * margin);
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
        private void AdjustQueriedAppBarDataRectangle(ref AppBarData appBarData, WinApiRectanglePoints desiredRectangle, MonitorInfoData currentMonitor)
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
        private bool AppBarDataRectangleIsDesired(AppBarData appBarData, WinApiRectanglePoints desiredRectangle, MonitorInfoData currentMonitor)
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


        #region Methods to handle events send by the handled window
        // ... 
        private void HandledWindowMouseEnter(object sender, MouseEventArgs e)
        {
            if (_currentAppBarAttributes.isRegisteredAutoHide)
            {
                HideUnhide(doHide: false);
            }
        }

        // ...
        private void HandledWindowMouseLeave(object sender, MouseEventArgs e)
        {
            if (_currentAppBarAttributes.isRegisteredAutoHide)
            {
                HideUnhide(doHide: true) ;
            }
        }

        private void HideUnhide(bool doHide = true)
        {

            // Get the rectangle information of the monitor the handled window is currently on.
            MonitorInfoData currentMonitor = HandleGetMonitorInfoFromWindowHandle();

            // Used in conjunction with _currentAppBarAttributes.margin to work out either the height or the width of the AppBar in pixels.
            int currentMonitorHeight = currentMonitor.rcMonitor.bottom - currentMonitor.rcMonitor.top;
            int currentMonitorWidth = currentMonitor.rcMonitor.right - currentMonitor.rcMonitor.left;

            // Determine the margin according to the doHide parameter.
            double margin;
            if (doHide)
            {
                margin = _currentAppBarAttributes.hiddenMargin;
            }
            else
            {
                margin = _currentAppBarAttributes.visibleMargin;
            }

            // Set the margin according to the screen edge the AppBar is bound to.
            switch (_currentAppBarAttributes.screenEdge)
            {
                case ScreenEdge.Left:
                    _windowToHandle.Width = currentMonitorWidth * margin;
                    break;
                case ScreenEdge.Top:
                    _windowToHandle.Height = currentMonitorHeight * margin;
                    break;
                case ScreenEdge.Right:
                    _windowToHandle.Width = currentMonitorWidth * margin;
                    _windowToHandle.Left = currentMonitor.rcMonitor.right - currentMonitorWidth * margin;
                    break;
                case ScreenEdge.Bottom:
                    _windowToHandle.Height = currentMonitorHeight * margin;
                    _windowToHandle.Top = currentMonitor.rcMonitor.bottom - currentMonitorHeight * margin;
                    break;
            }

            // Finally invert _currentAppBarAttributes.isHidden to reflect the new status of the AppBar. This could have already
            // been done in the previous if-statement. I prever it to be here though because it belongs here logically.
            if (doHide)
            {
                _currentAppBarAttributes.isHidden = true;
            }
            else
            {
                _currentAppBarAttributes.isHidden = false;
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

        // Retrieves a handle to the display monitor that contains a specified point.
        // This function is used to get the monitor handle for the 'GetMonitorInfo' function.
        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromPoint(WinApiPoint pt, int dwFlags);

        // Retrieves a handle to the display monitor that has the largest area of intersection with a specified rectangle.
        // This function is used to get the monitor handle for the 'GetMonitorInfo' function.
        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromRect(ref WinApiRectanglePoints lprc, int dwFlags);

        // Retrieves a handle to the display monitor that has the largest area of intersection with the bounding rectangle
        // of a specified window. This function is used to get the monitor handle for the 'GetMonitorInfo' function.
        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromWindow(IntPtr hWnd, int dwFlags);
        
        /// <summary>
        /// Moves the window to the position specified by the X, Y, nWidth and nHeight parameters.
        /// </summary>
        /// <param name="hWnd">Handle to the window which should be moved.</param>
        /// <param name="X">X-coordinate in pixels.</param>
        /// <param name="Y">Y.coordinate in pixels.</param>
        /// <param name="nWidth">Width in pixels.</param>
        /// <param name="nHeight">Height in pixels.</param>
        /// <param name="bRepaint">Indicates whether the window should be repainted after the function moved the window.</param>
        /// <returns></returns>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        
        /// <summary>
        /// Retrieves the position of the mouse cursor, in screen coordinates.
        /// </summary>
        /// <param name="pt">A reference to a WinApiPoint strcut that receives the screen coordinates of the cursor.</param>
        /// <returns>Returns true if successful and false otherwise.</returns>
        [DllImport("User32.dll")]
        private static extern bool GetCursorPos(ref WinApiPoint pt);
        #endregion
    }
}
