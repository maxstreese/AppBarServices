using System.Diagnostics;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using AppBarServices;
using AppBarServices.Enums;
using AppBarServices.Structs;

using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace TestStuffWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AppBarHandler _appBarHandler;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnPlaceTop_Click(object sender, RoutedEventArgs e)
        {
            _appBarHandler.PlaceAppBar(ScreenEdge.Top);
        }

        private void btnPlaceLeft_Click(object sender, RoutedEventArgs e)
        {
            _appBarHandler.PlaceAppBar(ScreenEdge.Left);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            _appBarHandler.RemoveAppBar();
        }

        private void btnPlaceRight_Click(object sender, RoutedEventArgs e)
        {
            _appBarHandler.PlaceAppBar(ScreenEdge.Right);
        }

        private void btnPlaceBottom_Click(object sender, RoutedEventArgs e)
        {
            _appBarHandler.PlaceAppBar(ScreenEdge.Bottom);
        }

        // **********************************************************************************************************

        private HwndSource _windowSource;
        private bool _isHooked;
        private PreviewWindow _previewWindow;

        private void btnStuffA_Click(object sender, RoutedEventArgs e)
        {
            DesiredAppBarAttributes desiredAppBarAttributes  = new DesiredAppBarAttributes();
            desiredAppBarAttributes.doAutoHide = true;
            desiredAppBarAttributes.doRegister = false;
            desiredAppBarAttributes.visibleMargin = 0.05;
            desiredAppBarAttributes.hiddenMargin = 0.005;
            desiredAppBarAttributes.screenEdge = ScreenEdge.Top;

            PreviewAttributes previewAttributes = new PreviewAttributes();
            previewAttributes.doPreviewToSnap = false;

            _appBarHandler = new AppBarHandler(this, desiredAppBarAttributes, previewAttributes);

            /*
            if (_windowSource == null)
            {
                WindowInteropHelper windowHelper = new WindowInteropHelper(this);
                _windowSource = HwndSource.FromHwnd(windowHelper.Handle);
            }

            if (!_isHooked)
            {
                _windowSource.AddHook(new HwndSourceHook(ProcessWinApiMessages));
                _isHooked = true;

            }
            */
        }

        private void btnStuffB_Click(object sender, RoutedEventArgs e)
        {
            /*
            if (_isHooked)
            {
                _windowSource.RemoveHook(new HwndSourceHook(ProcessWinApiMessages));
                _isHooked = false;
            }

            if (_previewWindow != null)
            {
                _previewWindow.Close();
            }
            */
        }

        /*
        private IntPtr ProcessWinApiMessages(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (uMsg == 534)
            {
                AppBarServices.Structs.WinApiPoint mousePosition = new AppBarServices.Structs.WinApiPoint();
                GetCursorPos(ref mousePosition);

                if (mousePosition.y < 100)
                {
                    if (_previewWindow == null)
                    {
                        _previewWindow = new PreviewWindow();
                        _previewWindow.Show();
                    }
                    else
                    {
                        if (_previewWindow.Visibility == Visibility.Collapsed)
                        {
                            _previewWindow.Visibility = Visibility.Visible;
                        }
                    }
                }
                else
                {
                    if (_previewWindow != null)
                    {
                        if (_previewWindow.Visibility == Visibility.Visible)
                        {
                            _previewWindow.Visibility = Visibility.Collapsed;
                        }
                    }
                }

                handled = true;
            }
            return IntPtr.Zero;
        }

        [DllImport("User32.dll")]
        private static extern bool GetCursorPos(ref AppBarServices.Structs.WinApiPoint pt);
        */
    }
}
