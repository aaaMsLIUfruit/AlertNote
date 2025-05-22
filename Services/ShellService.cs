using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using StickyAlerts.ViewModels;
using StickyAlerts.Views;

namespace StickyAlerts.Services
{
    public interface IShellService
    {
        public void ShowShell();
        public void HideShell();
        public void ShowTrayIcon();
        public void HideTrayIcon();
    }

    public class ShellService : IShellService
    {
        // 此处不能使用构造函数注入 ShellWIindow，因为 ShellViewModel 依赖于 ShellService

        public void ShowShell()
        {
            var shellWindow = GetShellWindow();
            if (shellWindow.Dispatcher.CheckAccess())
            {
                shellWindow.Show();
                shellWindow.Activate();
            }
            else
            {
                shellWindow.Dispatcher.Invoke(() =>
                {
                    shellWindow.Show();
                    shellWindow.Activate();
                });
            }
        }

        public void HideShell()
        {
            var shellWindow = GetShellWindow();
            var shellViewModel = GetShellViewModel();

            if (shellWindow.Dispatcher.CheckAccess())
            {
                shellViewModel.IsNotifyIconVisible = true;
                shellWindow.WindowState = System.Windows.WindowState.Minimized;
            }
            else
            {
                shellWindow.Dispatcher.Invoke(() =>
                {
                    shellViewModel.IsNotifyIconVisible = true;
                    shellWindow.WindowState = System.Windows.WindowState.Minimized;
                });
            }
        }

        public void ShowTrayIcon()
        {
            var shellWindow = GetShellWindow();
            var shellViewModel = GetShellViewModel();
            if (shellWindow.CheckAccess())
            {
                shellViewModel.IsNotifyIconVisible = true;
            }
            else
            {
                shellWindow.Dispatcher.Invoke(() =>
                {
                    shellViewModel.IsNotifyIconVisible = true;
                });
            } 
        }

        public void HideTrayIcon()
        {
            var shellWindow = GetShellWindow();
            var shellViewModel = GetShellViewModel();
            if (shellWindow.CheckAccess())
            {
                shellViewModel.IsNotifyIconVisible = false;
            }
            else
            {
                shellWindow.Dispatcher.Invoke(() =>
                {
                    shellViewModel.IsNotifyIconVisible = false;
                });
            }
            
        }

        private ShellWindow GetShellWindow()
        {
            return App.Host.Services.GetRequiredService<ShellWindow>();
        }

        private ShellViewModel GetShellViewModel()
        {
            return App.Host.Services.GetRequiredService<ShellViewModel>();
        }
    }
}
