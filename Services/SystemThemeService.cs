using Microsoft.Win32;

namespace StickyAlerts.Services
{
    public interface ISystemThemeService
    {
        public event EventHandler<SystemThemeChangedEventArgs>? SystemThemeChanged;
        public SystemTheme CurrentTheme => GetTheme();
        public SystemTheme GetTheme();
        public void StartMonitor();
        public void StopMonitor();
    }

    public class SystemThemeService : ISystemThemeService
    {
        private SystemTheme _oldTheme;
        private System.Timers.Timer _themeChangeTimer;

        public event EventHandler<SystemThemeChangedEventArgs>? SystemThemeChanged;
        public SystemTheme CurrentTheme => GetTheme();

        public SystemThemeService()
        {
            _themeChangeTimer = new System.Timers.Timer();
            _themeChangeTimer.Elapsed += ThemeChangeTimer_Elapsed;
            _themeChangeTimer.Interval = TimeSpan.FromSeconds(1).TotalMilliseconds;
            _themeChangeTimer.AutoReset = true;
            _oldTheme = GetTheme();
        }

        ~SystemThemeService()
        {
            _themeChangeTimer.Stop();
            _themeChangeTimer.Dispose();
        }

        public void StartMonitor()
        {
            _themeChangeTimer.Start();
        }

        public void StopMonitor()
        {
            _themeChangeTimer.Stop();
        }

        public SystemTheme GetTheme()
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            if (value is int i) return i > 0 ? SystemTheme.Light : SystemTheme.Dark; else return SystemTheme.Unknown;
        }

        private void ThemeChangeTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            var currentTheme = GetTheme();
            if (_oldTheme != currentTheme)
            {
                _oldTheme = currentTheme;
                SystemThemeChanged?.Invoke(this, new SystemThemeChangedEventArgs(currentTheme));
            }
        }
    }
    public class SystemThemeChangedEventArgs : EventArgs
    {
        public SystemTheme Theme { get; }

        public SystemThemeChangedEventArgs(SystemTheme theme)
        {
            Theme = theme;
        }
    }

    public enum SystemTheme
    {
        Unknown,
        Light,
        Dark,
    }
}
