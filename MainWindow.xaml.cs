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

namespace WpfApp26
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new VM();
        }

        // После открытия окна подтягиваем данные из базы
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            VM vm = DataContext as VM;
            if (vm != null)
            {
                await vm.InitializeAsync();
            }
        }

        // Перетаскивание окна за заголовок (окно без системной рамки)
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
                return;
            }

            DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Maximize_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Разворачивание с учётом рабочей области (чтобы окно не закрывало панель задач)
        private void ToggleMaximize()
        {
            if (WindowState == WindowState.Maximized)
            {
                MaxHeight = double.PositiveInfinity;
                MaxWidth = double.PositiveInfinity;
                WindowState = WindowState.Normal;
            }
            else
            {
                MaxHeight = SystemParameters.WorkArea.Height + 8;
                MaxWidth = SystemParameters.WorkArea.Width + 8;
                WindowState = WindowState.Maximized;
            }
        }
    }
}
