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

namespace WindowTesting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AppBarHandler _appBarHandler;
        double _appBarMargin = 0.25;

        public MainWindow()
        {
            InitializeComponent();
            _appBarHandler = new AppBarHandler(this);
            Closing += MainWindow_Closing;
        }

        private void btnPlaceTop_Click(object sender, RoutedEventArgs e)
        {
            _appBarHandler.PlaceAppBar(AppBarType.Standard, ScreenEdge.Top, _appBarMargin);
        }

        private void btnPlaceLeft_Click(object sender, RoutedEventArgs e)
        {
            _appBarHandler.PlaceAppBar(AppBarType.Standard, ScreenEdge.Left, _appBarMargin);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            _appBarHandler.RemoveAppBar();
        }

        private void btnPlaceRight_Click(object sender, RoutedEventArgs e)
        {
            _appBarHandler.PlaceAppBar(AppBarType.Standard, ScreenEdge.Right, _appBarMargin);
        }

        private void btnPlaceBottom_Click(object sender, RoutedEventArgs e)
        {
            _appBarHandler.PlaceAppBar(AppBarType.Standard, ScreenEdge.Bottom, _appBarMargin);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _appBarHandler.RemoveAppBar();
        }
    }
}
