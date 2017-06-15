// The class 'AppBarHandler' implements all of the logic of 'AppBarServices'. It is the only class defined with the public modifier
// and its intended use is for a WPF window to define it as a member and call its public methods in order to implement an AppBar. 

using System.Diagnostics;

using System;
using System.Windows;
using System.Windows.Media;
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
        /// Gets or sets the DoPreviewToSnap value of the AppBarHandler, that determines whether the Handler should show a preview window to show
        /// where the AppBar would be placed when the user drags the handled window to a screen edge, and places the AppBar at that position
        /// if the user stops dragging the window while near the screen edge.
        /// </summary>
        public bool DoPreviewToSnap
        {
            get { return _previewAttributes.doPreviewToSnap; }
            set
            {
                throw new NotImplementedException();
            }
        }
            
        /// <summary>
        /// Gets or sets the margin (in percent of screen pixels) from the screen edge at which the PreviewToSnap functionality of the
        /// AppBarHandler should kick in (i.e. show the preview window). Only used when the PreviewToSnap functionality of the Handler is being
        /// used.
        /// </summary>
        public double PreviewToSnapMargin
        {
            get { return _previewAttributes.previewToSnapMargin; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the value of the opacity of the preview window that is used to preview the AppBar position when the PreviewToSnap
        /// functionality of the AppBarHandler is being used.
        /// </summary>
        public double PreviewOpacity
        {
            get { return _previewAttributes.windowOpacity; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the value of the window background color of the preview window that is used to preview the AppBar position when
        /// the PreviewToSnap functionality of the AppBarHandler is being used.
        /// </summary>
        public Color PreviewBackgroundColor
        {
            get { return _previewAttributes.windowBackgroundColor; }
            set { throw new NotImplementedException(); } 
        }

        /// <summary>
        /// Gets or sets the value of the window border color of the preview window that is used to preview the AppBar position when
        /// the PreviewToSnap functionality of the AppBarHandler is being used.
        /// </summary>
        public Color PreviewBorderColor
        {
            get { return _previewAttributes.windowBorderColor; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets or sets the value of the window border thickness of the preview window that is used to preview the AppBar position when
        /// the PreviewToSnap functionality of the AppBarHandler is being used.
        /// </summary>
        public double PreviewBorderThickness
        {
            get { return _previewAttributes.windowBorderThickness; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Indicates whether the AppBar is currently registered or not.
        /// </summary>
        public bool AppBarIsRegistered
        {
            get { return _currentAppBarAttributes.isRegistered; }
        }
        
        /// <summary>
        /// Gets or sets the AutoHide value of the AppBar.
        /// </summary>
        public bool AppBarIsAutohide
        {
            get { return _currentAppBarAttributes.isRegisteredAutoHide; }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets or sets the margin of the AppBar that is used when it is not hidden. This value is used both by AppBars that are set to Autohide
        /// and by the ones that aren't.
        /// </summary>
        public double AppBarVisibleMargin
        {
            get { return _currentAppBarAttributes.visibleMargin; }
            set
            {
                throw new NotImplementedException();
            }
        }
        
        /// <summary>
        /// Gets or sets the margin of the AppBar that is used when it is hidden. This value is only used by AppBars that are set to AutoHide.
        /// </summary>
        public double AppBarHiddenMargin
        {
            get { return _currentAppBarAttributes.hiddenMargin; }
            set
            {
                throw new NotImplementedException();
            }
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
        /// Initializes the AppBarHandler. Note: The initialization of the AppBarHandler does not lead to a placed AppBar.
        /// This is either done by calling PlaceAppBar or by using the PreviewToSnap functionality of the AppBarHandler.
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
        #endregion


        #region Public Methods
        /// <summary>
        /// Registeres the handled window as an AppBar if it isn't already registered and places it on the specified edge of the screen the handled
        /// window is currently on.
        /// </summary>
        /// <param name="screenEdge">The edge of the screen at which the AppBar should be placed.</param>
        /// <returns>Returns true if the AppBar could be registered and placed, false otherwise.</returns>
        public bool PlaceAppBar(ScreenEdge screenEdge)
        {
            // The monitor the AppBar should be placed on is the monitor the handled window is currently on.
            _desiredAppBarAttributes.monitorToPlaceOn = GetMonitorInfoFromWindowHandle();

            // The supplied ScreenEdge is the new desired ScreenEdge.
            _desiredAppBarAttributes.screenEdge = screenEdge;

            // Check if the AppBar is already registered. If so only try to move it. Else first try to register it and then try to move it.
            if (!_currentAppBarAttributes.isRegistered)
            {
                // Since the handled window is not an AppBar right now but is about to become one, save the current attributes of the
                // handled window so that those can be restored once it unregisters as an AppBar again. And then prepare the handled
                // window to become an AppBar.
                SaveRestoreOriginalWindowAttributes(doSave: true);
                PrepareWindowToHandle();

                // We want to register the AppBar.
                _desiredAppBarAttributes.doRegister = true;

                // Check whether the operating system lets us register the handled window as an AppBar. If so proceed, if not
                // return false.
                if (SendAppBarNew())
                {
                    // Check whether the AppBar should AutoHide or not and proceed accordingly.
                    if (_desiredAppBarAttributes.doAutoHide)
                    {
                        // Try to register the AppBar as AutoHide. If that fails, unregister it and return false.
                        if (!SendSetAutoHideBarEx())
                        {
                            _desiredAppBarAttributes.doRegister = false;
                            SendAppBarRemove();

                            return false;
                        }

                        // Try to position the AppBar. If that fails first unregister it as AutoHide, then unregister it as AppBar
                        // and finally return false.
                        if (!SendAppBarQueryPosSetPos())
                        {
                            _desiredAppBarAttributes.doAutoHide = false;
                            SendSetAutoHideBarEx();

                            _desiredAppBarAttributes.doRegister = false;
                            SendAppBarRemove();

                            return false;
                        }
                    }
                    // AutoHide is not desired.
                    else
                    {
                        // Try to position the AppBar. If that fails unregister as AppBar and then return false.
                        if (!SendAppBarQueryPosSetPos())
                        {
                            _desiredAppBarAttributes.doRegister = false;
                            SendAppBarRemove();

                            return false;
                        }
                    }
                }
                // The AppBar could not be registered, therefore return false.
                else
                {
                    return false;
                }
            }
            // The AppBar is already registered.
            else
            {
                // Check whether the AppBar is registered as AutoHide or not and proceed accordingly.
                if (_currentAppBarAttributes.isRegisteredAutoHide)
                {
                    // Unregister the AppBar as AutoHide in case the screen and/or screen edge will change.
                    _desiredAppBarAttributes.doAutoHide = false;
                    SendSetAutoHideBarEx();

                    // Try to register the AppBar as AutoHide for the new position. If that fails, unregister the AppBar
                    // and return false.
                    _desiredAppBarAttributes.doAutoHide = true;
                    if (!SendSetAutoHideBarEx())
                    {
                        _desiredAppBarAttributes.doRegister = false;
                        SendAppBarRemove();

                        return false;
                    }

                    // Try to position the AppBar. If that fails first unregister it as AutoHide, then unregister it as AppBar
                    // and finally return false.
                    if (!SendAppBarQueryPosSetPos())
                    {
                        _desiredAppBarAttributes.doAutoHide = false;
                        SendSetAutoHideBarEx();

                        _desiredAppBarAttributes.doRegister = false;
                        SendAppBarRemove();

                        return false;
                    }
                }
                // The AppBar is not registered as AutoHide, therefore only try to move it.
                else
                {
                    // Try to position the AppBar. If that fails unregister as AppBar and then return false.
                    if (!SendAppBarQueryPosSetPos())
                    {
                        _desiredAppBarAttributes.doRegister = false;
                        SendAppBarNew();

                        return false;
                    }
                }
            }

            // All went well, so return true.
            return true;
        }

        /// <summary>
        /// Unregisters the handled window as an AppBar (if it is registered) and resets it to the position it was in before it registered as
        /// an AppBar.
        /// </summary>
        public void RemoveAppBar()
        {
            // Check whether the handled window is currently registered as an AppBar. If so proceed, if not there is nothing to do.
            if (_currentAppBarAttributes.isRegistered)
            {
                // Check whether the AppBar is currently registered as AutoHide. If so unregister as AutoHide.
                if (_currentAppBarAttributes.isRegisteredAutoHide)
                {
                    _desiredAppBarAttributes.doAutoHide = false;
                    SendSetAutoHideBarEx();
                }

                // Unregister as AppBar.
                _desiredAppBarAttributes.doRegister = false;
                SendAppBarRemove();

                // Restore the original window attributes (i.e. set the handled window back to the way it was before it became an AppBar.
                SaveRestoreOriginalWindowAttributes(doSave: false);
            }
        }
        #endregion


        #region Methods to encapsulate the WinApi - Retrieving MonitorInfoData
            /// <summary>
            /// Gets information in the form of a MonitorInfoData struct of the monitor that contains the point specified by the point parameter.
            /// If the specified point is not contained in any monitor, the method defaults to the primary monitor.
            /// </summary>
            /// <param name="point">The point for which the containing monitors MonitorInfoData should be retrieved.</param>
            /// <returns>Returns a MonitorInfoData struct.</returns>
            private MonitorInfoData GetMonitorInfoFromPoint(WinApiPoint point)
            {
                MonitorInfoData monitorInfoData = new MonitorInfoData();
                monitorInfoData.cbSize = Marshal.SizeOf(monitorInfoData);

                IntPtr monitorHandle;
                monitorHandle = MonitorFromPoint(point, (int)MonitorInfoOnNoIntersection.MONITOR_DEFAULTTOPRIMARY);
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
            internal MonitorInfoData GetMonitorInfoFromRect(WinApiRectanglePoints rectangle)
            {
                MonitorInfoData monitorInfoData = new MonitorInfoData();
                monitorInfoData.cbSize = Marshal.SizeOf(monitorInfoData);

                IntPtr monitorHandle;
                monitorHandle = MonitorFromRect(ref rectangle, (int)MonitorInfoOnNoIntersection.MONITOR_DEFAULTTOPRIMARY);
                GetMonitorInfo(monitorHandle, ref monitorInfoData);

                return monitorInfoData;
            }
            
            /// <summary>
            /// Gets information in the form of a MonitorInfoData struct of the monitor the handled window is currently on.
            /// If the handled window does not overlap with any monitor, the method defaults to the primary monitor.
            /// </summary>
            /// <returns>Returns a MonitorInfoData struct.</returns>
            internal MonitorInfoData GetMonitorInfoFromWindowHandle()
            {
                MonitorInfoData monitorInfoData = new MonitorInfoData();
                monitorInfoData.cbSize = Marshal.SizeOf(monitorInfoData);

                IntPtr monitorHandle;
                monitorHandle = MonitorFromWindow(_windowSource.Handle, (int)MonitorInfoOnNoIntersection.MONITOR_DEFAULTTOPRIMARY);
                GetMonitorInfo(monitorHandle, ref monitorInfoData);

                return monitorInfoData;
            }
        #endregion


        #region Methods to encapsulate the WinApi - SHAppBarMessage
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

            // Finally save that the AppBar is now registered and return true.
            _currentAppBarAttributes.isRegistered = true;
            return true;
        }
        
        /// <summary>
        /// Works out the position of the AppBar, reserves the space (i.e. calls SHAppBarData with the MessageIdentifiers ABM_QUERYPOS and ABM_SETPOS)
        /// and moves the WPF window to the reserved space if that space has the margin expected (i.e. defined) by the caller.
        /// </summary>
        /// <returns>Returns true if successful, false otherwise</returns>
        private bool SendAppBarQueryPosSetPos()
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
            SetInitialRectangle(ref appBarData, ref desiredRectangle);

            // Query the position where the AppBar should go and let the operating system adjust it for any obstacles.
            // Then take the adjusted values and correct them to represent the desired width or height value
            // (depending on the defined screen edge).
            SHAppBarMessage((int)MessageIdentifier.ABM_QUERYPOS, ref appBarData);
            AdjustQueriedAppBarDataRectangle(ref appBarData, desiredRectangle);

            // Reserve the position for the AppBar and then check if the position set by the operating system is equal in
            // width or height (depending on the defined screen edge) to the desired width or height value. If that is not
            // the case return false.
            uint testReturn;
            testReturn = SHAppBarMessage((int)MessageIdentifier.ABM_SETPOS, ref appBarData);
            if (!AppBarDataRectangleIsDesired(appBarData, desiredRectangle))
            {
                _currentAppBarAttributes.isHidden = false;
                return false;
            }

            // Move the WPF window to the reserved position. Do this using the MoveWindow function from the WinApi, because this looks smoother
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
            // Update the _curremtAppBarAttributes (monitorPlacedOn)
            _currentAppBarAttributes.monitorPlacedOn = _desiredAppBarAttributes.monitorToPlaceOn;
            // Update the _curremtAppBarAttributes (screenEdge)
            _currentAppBarAttributes.screenEdge = _desiredAppBarAttributes.screenEdge;
            // Update the _curremtAppBarAttributes (isHidden)
            _currentAppBarAttributes.isHidden = _desiredAppBarAttributes.doAutoHide;
            // Update the _currentAppBarAttributes (margins)
            _currentAppBarAttributes.visibleMargin = _desiredAppBarAttributes.visibleMargin;
            _currentAppBarAttributes.hiddenMargin = _desiredAppBarAttributes.hiddenMargin;

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
            
            _currentAppBarAttributes.isRegistered = false;
        }
        
        /// <summary>
        /// Registers or unregisters an AppBar as AutoHide (i.e. calls SHAppBarData with the MessageIdentifier ABM_SETAUTOHIDEBAREX).
        /// </summary>
        /// <returns>Returns true if successful, false otherwise.</returns>
        private bool SendSetAutoHideBarEx()
        {
            // The screen edge and monitor that will be used when sending the message.
            ScreenEdge screenEdge;
            MonitorInfoData monitorToSendFor;

            // If the AppBar should register as AutoHide, desired attributes should be used. If it should unregister as AutoHide
            // current attributes should be used.
            if (_desiredAppBarAttributes.doAutoHide)
            {
                screenEdge = _desiredAppBarAttributes.screenEdge;
                monitorToSendFor = _desiredAppBarAttributes.monitorToPlaceOn;
            }
            else
            {
                screenEdge = _currentAppBarAttributes.screenEdge;
                monitorToSendFor = _currentAppBarAttributes.monitorPlacedOn;
            }
            
            // Specifying the AppBarData to be supplied to the SHAppBarMessage function part 1.
            AppBarData appBarData = new AppBarData();
            appBarData.cbSize = Marshal.SizeOf(appBarData);
            appBarData.lParam = Convert.ToInt32(_desiredAppBarAttributes.doAutoHide);
            appBarData.uEdge = (int)_desiredAppBarAttributes.screenEdge;
            appBarData.hWnd = _windowSource.Handle;

            // Specifying the AppBarData to be supplied to the SHAppBarMessage function part 2.
            appBarData.rc.left = monitorToSendFor.rcMonitor.left;
            appBarData.rc.top = monitorToSendFor.rcMonitor.top;
            appBarData.rc.right = monitorToSendFor.rcMonitor.right;
            appBarData.rc.bottom = monitorToSendFor.rcMonitor.bottom;

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
        #endregion


        #region Methods to encapsulate the WinApi - WindowProc implementation
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
                    // System.Diagnostics.Debug.Print("hi");
                    
                    /*
                    SendAppBarQueryPosSetPos(doHide: _currentAppBarAttributes.isHidden);
                    handled = true;
                    */
                }
            }

            if (uMsg == 534)
            {
                // System.Diagnostics.Debug.Print("HalliHallo");
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
                
                MonitorInfoData originalMonitor = GetMonitorInfoFromWindowHandle();
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
                MonitorInfoData currentMonitor = GetMonitorInfoFromRect(windowRectangle);

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

        /// <summary>
        /// Sets all properties of the handled window to be AppBar suitable.
        /// </summary>
        private void PrepareWindowToHandle()
        {
            _windowToHandle.Topmost = true;
            _windowToHandle.WindowStyle = WindowStyle.None;
            _windowToHandle.ResizeMode = ResizeMode.NoResize;
            _windowToHandle.WindowState = WindowState.Normal;
        }

        // Sets the initial values for desiredRectangle and appBarData.rc depending on _currentAppBarAttributes.screenEdge and
        // _currentAppBarAttributes.margin.
        /// <summary>
        /// Sets the initial values for the desiredRectangle and appBarData.rc depending on the supplied parameters.
        /// </summary>
        /// <param name="appBarData">A reference to an AppBarData struct that gets filled by this method.</param>
        /// <param name="desiredRectangle">A reference to a WinApiRectanglePoints struct that gets filled by this method.</param>
        /// <param name="desiredMonitor">A MonitorInfoData struct containing information about the monitor the AppBar should be placed on.</param>
        private void SetInitialRectangle(ref AppBarData appBarData, ref WinApiRectanglePoints desiredRectangle)
        {
            // Used in conjunction with _currentAppBarAttributes.margin to work out either the height or the width of the AppBar in pixels.
            int desiredMonitorHeight = _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.bottom - _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.top;
            int desiredMonitorWidth = _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.right - _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.left;
            
            // Determine, whether the AppBar should be treated as hidden or not and set the margin to be used accordingly.
            double margin;
            if (_desiredAppBarAttributes.doAutoHide)
            {
                margin = _desiredAppBarAttributes.hiddenMargin;
            }
            else
            {
                margin = _desiredAppBarAttributes.visibleMargin;
            }
            
            // Check which screen to bound the AppBar to and then set the rectangle values.
            if (_desiredAppBarAttributes.screenEdge == ScreenEdge.Left || _desiredAppBarAttributes.screenEdge == ScreenEdge.Right)
            {
                desiredRectangle.top = _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.top;
                desiredRectangle.bottom = _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.bottom;
                appBarData.rc.top = desiredRectangle.top;
                appBarData.rc.bottom = desiredRectangle.bottom;
                if (_desiredAppBarAttributes.screenEdge == ScreenEdge.Left)
                {
                    desiredRectangle.left = _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.left;
                    desiredRectangle.right = _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.left + (int)(desiredMonitorWidth * margin);
                    appBarData.rc.left = desiredRectangle.left;
                    appBarData.rc.right = desiredRectangle.right;
                }
                else
                {
                    desiredRectangle.right = _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.right;
                    desiredRectangle.left = _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.right - (int)(desiredMonitorWidth * margin);
                    appBarData.rc.right = desiredRectangle.right;
                    appBarData.rc.left = desiredRectangle.left;
                }
            }
            else
            {
                desiredRectangle.left = _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.left;
                desiredRectangle.right = _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.right;
                appBarData.rc.left = desiredRectangle.left;
                appBarData.rc.right = desiredRectangle.right;
                if (_desiredAppBarAttributes.screenEdge == ScreenEdge.Top)
                {
                    desiredRectangle.top = _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.top;
                    desiredRectangle.bottom = _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.top + (int)(desiredMonitorHeight * margin);
                    appBarData.rc.top = desiredRectangle.top;
                    appBarData.rc.bottom = desiredRectangle.bottom;
                }
                else
                {
                    desiredRectangle.bottom = _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.bottom;
                    desiredRectangle.top = _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.bottom - (int)(desiredMonitorHeight * margin);
                    appBarData.rc.bottom = desiredRectangle.bottom;
                    appBarData.rc.top = desiredRectangle.top;
                }
            }
        }

        /// <summary>
        /// Adjusts for any deviations between AppBarData.rc and desiredRectangle.
        /// </summary>
        /// <param name="appBarData">A reference to an AppBarData struct. Values in AppBarData.rc might be changed by this method.</param>
        /// <param name="desiredRectangle">A WinApiPoints struct conatining the dimensions and sposition of the desired rectangle.</param>
        /// <remarks>
        /// After the ABM_QUERYPOS message has been send to the operating system, this method tries to adjust for any changes made by the system
        /// to the AppBarData.rc rectangle. An example for why this is needed: If the AppBar is supposed to go to the bottom of the screen and
        /// the taskbar is located there, the system will adjust the desired AppBar position so that it doesnt overlap the taskbar. It will do this
        /// by setting the bottom of the desired rectangle to the taskbar top. And leave the desired top position of the AppBar rectangle unchanged
        /// (if it doesnt overlap the taskbar as well, in that case it will be set to the taskbar top as well). This method tries to adjust this
        /// change so that the width or height (depending on the defined screen edge) value of the AppBar is unchanged by the system correction.
        /// </remarks>
        private void AdjustQueriedAppBarDataRectangle(ref AppBarData appBarData, WinApiRectanglePoints desiredRectangle)
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
        private bool AppBarDataRectangleIsDesired(AppBarData appBarData, WinApiRectanglePoints desiredRectangle)
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
                    if (appBarData.rc.right > _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.right)
                    {
                        return false;
                    }
                }
                else
                {
                    if (appBarData.rc.left < _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.left)
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
                    if (appBarData.rc.bottom > _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.bottom)
                    {
                        return false;
                    }
                }
                else
                {
                    if (appBarData.rc.top < _desiredAppBarAttributes.monitorToPlaceOn.rcMonitor.top)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void HideUnhide(bool doHide = true)
        {
            // Get the rectangle information of the monitor the handled window is currently on.
            MonitorInfoData currentMonitor = GetMonitorInfoFromWindowHandle();

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


        #region Event handlers for events send by the handled window
        /// <summary>
        /// Event handler for the MouseEnter event of the handled window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>This handler is used enable hiding/unhiding an AppBar that is set to AutoHide.</remarks>
        private void HandledWindowMouseEnter(object sender, MouseEventArgs e)
        {
            if (_currentAppBarAttributes.isRegisteredAutoHide)
            {
                HideUnhide(doHide: false);
            }
        }

        /// <summary>
        /// Event handler for the MouseLeave event of the handled window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>This handler is used enable hiding/unhiding an AppBar that is set to AutoHide.</remarks>
        private void HandledWindowMouseLeave(object sender, MouseEventArgs e)
        {
            if (_currentAppBarAttributes.isRegisteredAutoHide)
            {
                HideUnhide(doHide: true) ;
            }
        }

        /// <summary>
        /// Event handler for the WindowClosed event of the handled window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>This handler is used to perform all necessary cleanup actions the AppBarHandler needs to perform
        /// before the handled window is closed.</remarks>
        private void HandledWindowClosed(object sender, EventArgs e)
        {
            RemoveAppBar();

            _windowSource.RemoveHook(new HwndSourceHook(ProcessWinApiMessages));
        }
        #endregion


        #region WinApi function declarations
        /// <summary>
        /// Sends an AppBar message to the operating system.
        /// </summary>
        /// <param name="dwMessage">The message identifier that specifies which message is being send. The values can be found in the
        /// enum MessageIdentifier.</param>
        /// <param name="pData">A reference (pointer) to an AppBarData structure. The content of the structure depends on the message
        /// being send.</param>
        /// <returns>Returns a message dependent value (usually 1 (true) if successful and 0 (false) otherwise).</returns>
        /// <remarks>
        /// This function registers and unregisters the AppBar with the operating system, queries and reserves screen space for it
        /// and registers or unregisters the AppBar as AutoHide (and some other things).
        /// The microsoft docs state that for some messages (e.g. ABM_REMOVE) this method always returns true (1). However if the C#
        /// implementation is incorrect (e.g. wrong types in AppBarData) the function will return false (0) instead.
        /// </remarks>
        [DllImport("SHELL32", CallingConvention = CallingConvention.StdCall)]
        private static extern uint SHAppBarMessage(int dwMessage, ref AppBarData pData);
        
        /// <summary>
        /// Registers a message value with the operating system, that is guaranteed to be unique throughout the system for a given 'msg' string.
        /// </summary>
        /// <param name="msg">The message to be registered.</param>
        /// <returns>
        /// If the message is successfully registered, the return value is a message identifier in the range 0xC000 through 0xFFFF. If the
        /// function fails, the return value is zero.
        /// </returns>
        /// <remarks>This function is needed in order for the AppBar to be able to receive notifications from the operating system.</remarks>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern uint RegisterWindowMessage(string msg);

        /// <summary>
        /// Retrieves information about a display monitor and stores that information in a 'MonitorInfoData' struct.
        /// </summary>
        /// <param name="hMonitor">A handle to the display monitor of interest.</param>
        /// <param name="lpmi">A reference (pointer) to a MonitorInfoData struct.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
        /// <remarks>This function is needed to place the AppBar properly on multi-monitor setups.</remarks>
        [DllImport("User32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoData lpmi);

        /// <summary>
        /// Retrieves a handle to the display monitor that contains a specified point.
        /// </summary>
        /// <param name="pt">A WinApiPoint struct that specifies the point of interest in virtual-screen coordinates.</param>
        /// <param name="dwFlags">Determines the function's return value if the point is not contained within any display monitor.
        /// The possible values are listed in MonitorInfoOnNoIntersection.</param>
        /// <returns>
        /// If the point is contained by a display monitor, the return value is an HMONITOR handle to that display monitor.
        /// If the point is not contained by a display monitor, the return value depends on the value of dwFlags.
        /// </returns>
        /// <remarks>This function is used to get the monitor handle for the 'GetMonitorInfo' function.</remarks>
        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromPoint(WinApiPoint pt, int dwFlags);

        /// <summary>
        /// Retrieves a handle to the display monitor that has the largest area of intersection with a specified rectangle.
        /// </summary>
        /// <param name="lprc">A pointer to a WinApiRectanglePoints struct that specifies the rectangle of interest in 
        /// virtual-screen coordinates.</param>
        /// <param name="dwFlags">Determines the function's return value if the rectangle is not contained within any display
        /// monitor. The possible values are listed in MonitorInfoOnNoIntersection.</param>
        /// <returns>
        /// If the point is contained by a display monitor, the return value is an HMONITOR handle to that display monitor.
        /// If the point is not contained by a display monitor, the return value depends on the value of dwFlags.
        /// </returns>
        /// <remarks>This function is used to get the monitor handle for the 'GetMonitorInfo' function.</remarks>
        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromRect(ref WinApiRectanglePoints lprc, int dwFlags);
        
        /// <summary>
        /// Retrieves a handle to the display monitor that has the largest area of intersection with the bounding rectangle
        /// of a specified window.
        /// </summary>
        /// <param name="hWnd">A handle to the window of interest.</param>
        /// <param name="dwFlags">Determines the function's return value if the window is not contained within any display
        /// monitor. The possible values are listed in MonitorInfoOnNoIntersection.</param>
        /// <returns></returns>
        /// <remarks>This function is used to get the monitor handle for the 'GetMonitorInfo' function.</remarks>
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
        /// <returns>
        /// If the function succeeds, the return value is nonzero. 
        /// If the function fails, the return value is zero.</returns>
        /// <remarks>
        /// This function is used to move the handled window because it does so smoother compared to moving the
        /// handled window by changing its left, top, width and height values via WPF.
        /// </remarks>
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        /// <summary>
        /// Retrieves the position of the mouse cursor, in screen coordinates.
        /// </summary>
        /// <param name="pt">A reference to a WinApiPoint strcut that receives the screen coordinates of the cursor.</param>
        /// <returns>Returns true if successful, false otherwise.</returns>
        /// <remarks>
        /// This function is used to get the position of the mouse cursor when the handled window is being moved in order to
        /// enable the PreviewToSnap funcionality of the AppBarHandler. Please note that I do not know of any way to do this
        /// using WPF only. There is one solution that is often suggested for this which is using the PointToScreen method.
        /// While this method generally is capable of returning the position of the mouse cursor, it does not do this when
        /// the mouse is dragging the window. In this case the returned mouse cursor position is always (0,0). Therefore I
        /// am using this WinApi function.
        /// </remarks>
        [DllImport("User32.dll")]
        private static extern bool GetCursorPos(ref WinApiPoint pt);
        #endregion
    }
}
