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
    /// <summary>
    /// ShellWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ShellWindow : HandyControl.Controls.Window
    {
        public ShellWindow()
        {
            InitializeComponent();
            
            // 从依赖注入容器获取ShellViewModel
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
    }
}
