using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StickyAlerts.Controls;
using StickyAlerts.Models;
using StickyAlerts.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace StickyAlerts.ViewModels
{
    public partial class SettingsViewModel : ObservableObject
    {
        private readonly ILogger _logger;
        private readonly ISettingsService<UserSettings> _userSettingsService;

        [Category("显示")]
        [DisplayName("语言")]
        public Language Language
        {
            get => _userSettingsService.Current.Language switch { "zh-CN" => Language.简体中文, "en-US" => Language.English, _ => Language.English, };
            set => SetUserSettingsProperty(_userSettingsService.Current.Language, value switch { Language.简体中文 => "zh-CN", Language.English => "en-US", _ => "en-US", }, true);
        }

        [Category("显示")]
        [DisplayName("主题")]
        public Theme Theme
        {
            get => _userSettingsService.Current.Theme switch { "Light" => Theme.Light, "Dark" => Theme.Dark, _ => Theme.Light, };
            set => SetUserSettingsProperty(_userSettingsService.Current.Theme, value switch { Theme.Light => "Light", Theme.Dark => "Dark", _ => "Light", }, true);
        }

        [Category("显示")]
        [DisplayName("便笺置顶")]
        public bool Topmost
        {
            get => _userSettingsService.Current.Topmost;
            set => SetUserSettingsProperty(_userSettingsService.Current.Topmost, value, true);
        }

        [Category("显示")]
        [DisplayName("水平间距")]
        public double HorizontalSpacing
        {
            get => _userSettingsService.Current.HorizontalSpacing;
            set => SetUserSettingsProperty(_userSettingsService.Current.HorizontalSpacing, value, true);
        }

        [Category("显示")]
        [DisplayName("垂直间距")]
        public double VerticalSpacing
        {
            get => _userSettingsService.Current.VerticalSpacing;
            set => SetUserSettingsProperty(_userSettingsService.Current.VerticalSpacing, value, true);
        }

        [Category("系统")]
        [DisplayName("开机自启")]
        public bool AutoStart
        {
            get => _userSettingsService.Current.AutoStart;
            set => SetUserSettingsProperty(_userSettingsService.Current.AutoStart, value, true);
        }

        [Category("系统")]
        [DisplayName("启动时隐藏主窗口")]
        public bool HideShell
        {
            get => _userSettingsService.Current.HideShell;
            set => SetUserSettingsProperty(_userSettingsService.Current.HideShell, value, true);
        }

        [Category("系统")]
        [DisplayName("显示托盘图标")]
        public bool ShowTrayIcon
        {
            get => _userSettingsService.Current.ShowTrayIcon;
            set => SetUserSettingsProperty(_userSettingsService.Current.ShowTrayIcon, value, true);
        }

        [Category("系统")]
        [DisplayName("数据保存路径")]
        [Editor(typeof(PathTextPropertyEditor), typeof(PathTextPropertyEditor))]
        public string? AlertsPath
        {
            get => _userSettingsService.Current.AlertsPath;
            set => SetUserSettingsProperty(_userSettingsService.Current.AlertsPath, value, true);
        }

        public SettingsViewModel(ISettingsService<UserSettings> userConfig, ILogger<SettingsViewModel> logger)
        {
            _logger = logger;
            _userSettingsService = userConfig;
            _userSettingsService.OnFileChanged((oldValue, newValue, name) =>
            {
                SetUserSettingsProperty(oldValue, newValue, false, name);
            });
        }

        private void SetUserSettingsProperty<T>(T oldValue, T newValue, bool save = false, [CallerMemberName] string? propertyName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(propertyName)) throw new ArgumentException("Property name is null or empty");
                SetProperty(
                    oldValue,
                    newValue,
                    _userSettingsService.Current,
                    (model, value) =>
                    {
                        var property = typeof(UserSettings).GetProperty(propertyName) ?? throw new ArgumentException($"Property {propertyName} not found");
                        property.SetValue(model, value);
                        OnPropertyChanged(propertyName);
                    },
                    propertyName);
                if (save) _userSettingsService.Save();
                _userSettingsService.Apply(propertyName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to set user settings property.");
            }
        }
    }

    /// <summary>
    /// 语言枚举
    /// </summary>
    public enum Language
    {
        [Description("简体中文")]
        简体中文,
        [Description("English")]
        English
    }

    /// <summary>
    /// 主题枚举
    /// </summary>
    public enum Theme
    {
        [Description("明亮")]
        Light,
        [Description("黑暗")]
        Dark
    }
}
