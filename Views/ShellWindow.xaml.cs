using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using StickyAlerts.Models;
using StickyAlerts.Services;
using StickyAlerts.ViewModels;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace StickyAlerts.Views
{

    public partial class ShellWindow : HandyControl.Controls.Window
    {
        public ShellWindow()
        {
            InitializeComponent();
            
            var shellViewModel = App.Host.Services.GetRequiredService<ShellViewModel>();
            DataContext = shellViewModel;
        }

        private void NotifyIcon_MouseDoubleClick(object sender, System.Windows.RoutedEventArgs e)
        {
            Show();
            Activate();
        }

        private void ShowShell_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Show();
            Activate();
        }

        private void Exit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void ImportHomeworkMenu_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ImportHomeworkDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void AddMenuButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            if (button?.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }

        private void ShowCalendarView_Click(object sender, RoutedEventArgs e)
        {
            var win = new Window
            {
                Title = "日历视图",
                Content = new StickyAlerts.Views.CalendarView(),
                Width = 600,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };
            win.ShowDialog();
        }
    }
}
