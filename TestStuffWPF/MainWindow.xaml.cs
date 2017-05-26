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

namespace TestStuffWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AppBarHandler _appBarHandler;
        double _appBarMargin = 1;

        public MainWindow()
        {
            InitializeComponent();
            _appBarHandler = new AppBarHandler(this);
            Closing += MainWindow_Closing;
        }

        private void btnPlaceTop_Click(object sender, RoutedEventArgs e)
        {
            if (_appBarHandler.AppBarIsRegistered == false)
            {
                _appBarHandler.PlaceAppBar(false, ScreenEdge.Top, _appBarMargin);
            }
            else
            {
                _appBarHandler.MoveAppBar(ScreenEdge.Top, _appBarMargin);
            }
        }

        private void btnPlaceLeft_Click(object sender, RoutedEventArgs e)
        {
            if (_appBarHandler.AppBarIsRegistered == false)
            {
                _appBarHandler.PlaceAppBar(false, ScreenEdge.Left, _appBarMargin);
            }
            else
            {
                _appBarHandler.MoveAppBar(ScreenEdge.Left, _appBarMargin);
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            _appBarHandler.RemoveAppBar();
        }

        private void btnPlaceRight_Click(object sender, RoutedEventArgs e)
        {
            if (_appBarHandler.AppBarIsRegistered == false)
            {
                _appBarHandler.PlaceAppBar(false, ScreenEdge.Right, _appBarMargin);
            }
            else
            {
                _appBarHandler.MoveAppBar(ScreenEdge.Right, _appBarMargin);
            }
        }

        private void btnPlaceBottom_Click(object sender, RoutedEventArgs e)
        {
            if (_appBarHandler.AppBarIsRegistered == false)
            {
                _appBarHandler.PlaceAppBar(false, ScreenEdge.Bottom, _appBarMargin);
            }
            else
            {
                _appBarHandler.MoveAppBar(ScreenEdge.Bottom, _appBarMargin);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _appBarHandler.RemoveAppBar();
        }

        private void btnStuffA_Click(object sender, RoutedEventArgs e)
        {
            // ...
        }

        private void btnStuffB_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.Print("WPF Left: {0}", this.Left);
            System.Diagnostics.Debug.Print("WPF Top: {0}", this.Top);
        }
    }
}
