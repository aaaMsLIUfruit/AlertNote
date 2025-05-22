using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StickyAlerts.Models;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Windows;
using System.IO;
using HandyControl.Themes;
using StickyAlerts.ViewModels;
using System.Windows.Threading;

namespace StickyAlerts.Services
{
    public interface ISettingsService<TModel> where TModel : class
    {
        public TModel Current { get; }
        public void Save();
        public void Apply(string name);
        public void ApplyAll();
        public void OnFileChanged(Action<object?, object?, string> callback);
    }

    public class UserSettingsService : ISettingsService<UserSettings>
    {
        private readonly ILogger _logger;
        private readonly ISystemThemeService _systemThemeService;
        private readonly IOptionsMonitor<UserSettings> _userSettingsMonitor;
        private Action<object?, object?, string>? _onFileChangedCallback;

        private UserSettings Previous;
        public UserSettings Current { get => _userSettingsMonitor.CurrentValue; }
        public UserSettingsService(ILogger<UserSettingsService> logger, IOptionsMonitor<UserSettings> userSettingsMonitor, ISystemThemeService systemThemeService)
        {
            _logger = logger;
            _systemThemeService = systemThemeService;
            _userSettingsMonitor = userSettingsMonitor;
            _userSettingsMonitor.OnChange(HandleFileChanged);

            Previous = DeepCopy(Current);

            _systemThemeService.SystemThemeChanged += HandleSystemThemeChanged;
        }

        public void ApplyAll()
        {
            foreach (var property in typeof(UserSettings).GetProperties())
            {
                Apply(property.Name);
            }
        }


        public void Apply(string name)
        {
            try
            {
                _logger.LogInformation("Try apply {name}", name);
                if (string.IsNullOrEmpty(name)) throw new ArgumentException("Property name is null or empty");
                var property = typeof(UserSettings).GetProperty(name) ?? throw new ArgumentException($"Property {name} not found");
                var value = property.GetValue(Current);
                switch (property.Name)
                {
                    case nameof(UserSettings.Language):
                        if (value is string language) ApplyLanguage(language);
                        break;
                    case nameof(UserSettings.Theme):
                        if (value is string theme) ApplyTheme(theme);
                        break;
                    case nameof(UserSettings.Topmost):
                        if (value is bool topmost) ApplyTopmost(topmost);
                        break;
                    case nameof(UserSettings.HorizontalSpacing):
                        if (value is double horizontalSpacing) ApplyHorizontalSpacing(horizontalSpacing);
                        break;
                    case nameof(UserSettings.VerticalSpacing):
                        if (value is double verticalSpacing) ApplyVerticalSpacing(verticalSpacing);
                        break;
                    case nameof(UserSettings.AutoStart):
                        if (value is bool autoStart) ApplyAutoStart(autoStart);
                        break;
                    case nameof(UserSettings.HideShell):
                        if (value is bool hideShell) break;
                        break;
                    case nameof(UserSettings.ShowTrayIcon):
                        if (value is bool showTrayIcon) ApplyShowTrayIcon(showTrayIcon);
                        break;
                    case nameof(UserSettings.AlertsPath):
                        if (value is string alertPath) ApplyAlertsPath(alertPath);
                        break;
                }
                _logger.LogInformation("Apply {name} success", name);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Apply {name} failed", name);
            }
        }

        public void Save()
        {
            try
            {
                _logger.LogInformation("Try saving user settings");
                var settingsJson = File.ReadAllText("Settings.json");
                var settingsNode = JsonNode.Parse(settingsJson);
                settingsNode["UserSettings"] = JsonNode.Parse(JsonSerializer.Serialize(Current));
                settingsJson = JsonSerializer.Serialize(settingsNode, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText("Settings.json", settingsJson);
                _logger.LogInformation("User settings saved");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to save user settings: {exception}", e.Message);
            }
        }

        public void OnFileChanged(Action<object?, object?, string> callback)
        {
            _onFileChangedCallback = callback;
        }

        #region 私有方法
        private void HandleFileChanged(UserSettings userSettings)
        {
            // 由于 Configuration 过于健壮，导致配置文件被修改成非法值都没有关系（不会触发该方法），所以无需做过多处理
            try
            {
                int count = 0;
                // 使用反射遍历所有属性
                foreach (var property in typeof(UserSettings).GetProperties())
                {
                    var oldValue = property.GetValue(Previous);
                    var newValue = property.GetValue(userSettings);

                    if (!object.Equals(oldValue, newValue))
                    {
                        count++;
                        _onFileChangedCallback?.Invoke(oldValue, newValue, property.Name);
                    }
                }
                _logger.LogInformation("Settings file changed, {count} differences", count);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to update settings: {exception}", e.Message);
            }
        }

        private void HandleSystemThemeChanged(object? sender, SystemThemeChangedEventArgs e)
        {
            if (_userSettingsMonitor.CurrentValue.Theme != "System") return;

            var handyTheme = e.Theme switch
            {
                SystemTheme.Light => HandyControl.Data.SkinType.Violet,
                SystemTheme.Dark => HandyControl.Data.SkinType.Dark,
                _ => HandyControl.Data.SkinType.Violet
            };

            // check access and invoke
            if (Application.Current.Dispatcher.CheckAccess())
            {
                foreach (Window window in Application.Current.Windows)
                {
                    HandyControl.Themes.Theme.SetSkin(window, handyTheme);
                }
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (Window window in Application.Current.Windows)
                    {
                        HandyControl.Themes.Theme.SetSkin(window, handyTheme);
                    }
                });
            }
        }

        private T DeepCopy<T>(T source) where T : class
        {
            // 基于序列化的深拷贝
            var json = JsonSerializer.Serialize(source) ?? throw new InvalidOperationException("Failed to deep copy in serialize progress");
            var target = JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException("Failed to deep copy in deserialize progress");
            return target;
        }

        private void ApplyAlertsPath(string alertsPath)
        {
            _logger.LogWarning("AlertsPath is not implemented");
        }

        private void ApplyAutoStart(bool autoStart)
        {
            _logger.LogWarning("AutoStart is not implemented");
        }

        private void ApplyHorizontalSpacing(double horizontalSpacing)
        {
            var alertService = App.Host.Services.GetRequiredService<IAlertService>();
            alertService.Align();
        }

        private void ApplyVerticalSpacing(double verticalSpacing)
        {
            var alertService = App.Host.Services.GetRequiredService<IAlertService>();
            alertService.Align();
        }

        private void ApplyLanguage(string language)
        {
            _logger.LogWarning("Language is not implemented");
        }

        private void ApplyShowTrayIcon(bool showTrayIcon)
        {
            var shellService = App.Host.Services.GetRequiredService<IShellService>();
            if (showTrayIcon) shellService.ShowTrayIcon(); else shellService.HideTrayIcon();
        }

        private void ApplyTheme(string theme)
        {
            var handyTheme = HandyControl.Data.SkinType.Violet;
            switch (theme)
            {
                case "Light":
                    handyTheme = HandyControl.Data.SkinType.Violet;
                    _systemThemeService.StopMonitor();
                    break;
                case "Dark":
                    handyTheme = HandyControl.Data.SkinType.Dark;
                    _systemThemeService.StopMonitor();
                    break;
                case "System":
                    var systemTheme = _systemThemeService.GetTheme();
                    handyTheme = systemTheme switch
                    {
                        SystemTheme.Light => HandyControl.Data.SkinType.Violet,
                        SystemTheme.Dark => HandyControl.Data.SkinType.Dark,
                        _ => HandyControl.Data.SkinType.Violet
                    };
                    _systemThemeService.StartMonitor();
                    break;
                default:
                    handyTheme = HandyControl.Data.SkinType.Violet;
                    break;
            }

            foreach (Window window in App.Current.Windows)
            {
                HandyControl.Themes.Theme.SetSkin(window, handyTheme);
            }
        }

        private void ApplyTopmost(bool topmost)
        {
            var alertService = App.Host.Services.GetRequiredService<IAlertService>();
            if (topmost)
            {
                foreach (var alert in alertService.Alerts)
                {
                    alert.Topmost = true;
                }
            }
            else
            {
                foreach (var alert in alertService.Alerts)
                {
                    alert.Topmost = false;
                }
            }
        }
        #endregion
    }
}
