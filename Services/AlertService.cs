using HandyControl.Tools.Extension;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StickyAlerts.Models;
using StickyAlerts.ViewModels;
using StickyAlerts.Views;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace StickyAlerts.Services
{
    public interface IAlertService
    {
        public ObservableCollection<AlertViewModel> Alerts { get; }
        public void Align();
        public void Load();
        public void Save();
        public void Add();
        public void Add(string title, string note, DateTime deadline, bool alertVisible = true, bool countdownVisible = true, bool noteVisible = false);
        public void Add(string title, string note, DateTime deadline, string theme, bool alertVisible = true, bool countdownVisible = true, bool noteVisible = false);
        public void Delete(Guid id);
        public void Delete(AlertViewModel alert);
    }

    public class AlertService : IAlertService
    {
        private List<Alert> _alerts;
        private ConcurrentDictionary<Guid, AlertWindow> _alertWindows;
        private ISettingsService<UserSettings> _userSettings;
        private ILogger<AlertService> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ObservableCollection<AlertViewModel> Alerts { get; private set; }

        public AlertService(ISettingsService<UserSettings> userSettings, ILogger<AlertService> logger)
        {
            _alerts = [];
            _alertWindows = new ConcurrentDictionary<Guid, AlertWindow>();
            _userSettings = userSettings;
            _logger = logger;
            _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true, };

            Alerts = [];
            Alerts.CollectionChanged += (sender, e) => Save();
            Load();
        }

        public void Load()
        {
            // 读取本地文件
            var directoryPath = _userSettings.Current.AlertsPath;
            try
            {
                var filePath = Path.Combine(directoryPath, "Alerts.json");
                if (Directory.Exists(_userSettings.Current.AlertsPath))
                {
                    // 如果指定路径存在，则读取文件
                    var json = File.ReadAllText(filePath);
                    var alerts = JsonSerializer.Deserialize<List<Alert>>(json);
                    if (alerts != null) _alerts = alerts; else File.WriteAllText(filePath, JsonSerializer.Serialize(_alerts, _jsonSerializerOptions));
                }
                else
                {
                    // 如果指定路径不存在，则创建目录与文件
                    _logger.LogWarning("Failed to load alerts from {filePath}, directory not exists, try create a empty file", filePath);
                    Directory.CreateDirectory(directoryPath);
                    File.WriteAllText(filePath, JsonSerializer.Serialize(_alerts, _jsonSerializerOptions));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to load alerts from {directoryPath}: {exception}, please check your setting", directoryPath, e.Message);
            }

            // 创建便笺窗体
            foreach (var alert in _alerts)
            {
                var vm = new AlertViewModel(alert);
                vm.PropertyChanged += (sender, e) => Save();
                Alerts.Add(vm);
                AddAlertWindow(vm);
            }
            Align();
            Save();
        }

        private void AddAlertWindow(AlertViewModel alertViewModel)
        {
            var alertWindow = new AlertWindow { DataContext = alertViewModel };
            alertWindow.UpdateLayout();
            // 愚蠢的方法，但确实有用
            alertWindow.Show();
            //alertWindow.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            alertViewModel.Width = alertWindow.ActualWidth;
            alertViewModel.Height = alertWindow.ActualHeight;
            Save();
            if (!alertViewModel.AlertVisible) alertWindow.Hide();
            _alertWindows.TryAdd(alertViewModel.Id, alertWindow);
        }

        public void Add()
        {
            Add("新的便笺", string.Empty, DateTime.Today.AddDays(1).AddHours(9), true, true, false);
        }

        public void Add(string title, string note, DateTime deadline, bool alertVisible = true, bool countdownVisible = true, bool noteVisible = false)
        {
            var vm = new AlertViewModel(new Alert()
            {
                Id = Guid.NewGuid(),
                Title = title,
                Note = note,
                Deadline = deadline,
                LastModified = DateTime.Now,
                AlertVisible = alertVisible,
                NoteVisible = noteVisible,
                CountdownVisible = countdownVisible,
            });
            Alerts.Add(vm);
            SortByDeadlineAndActiveState(Alerts);
            AddAlertWindow(vm);
            Save();
            Align();
        }

        public void Add(string title, string note, DateTime deadline, string theme, bool alertVisible = true, bool countdownVisible = true, bool noteVisible = false)
        {
            var vm = new AlertViewModel(new Alert()
            {
                Id = Guid.NewGuid(),
                Title = title,
                Note = note,
                Deadline = deadline,
                LastModified = DateTime.Now,
                AlertVisible = alertVisible,
                NoteVisible = noteVisible,
                CountdownVisible = countdownVisible,
                Theme = theme,
            });
            Alerts.Add(vm);
            SortByDeadlineAndActiveState(Alerts);
            // AddAlertWindow(vm); // 不再自动弹窗
            Save();
            Align();
        }

        public void Delete(Guid id)
        {
            if (_alertWindows.TryRemove(id, out var alertWindow)) alertWindow.Close();
            Alerts.Remove(Alerts.First(a => a.Id == id));
            SortByDeadlineAndActiveState(Alerts);
            Save();
            Align();
        }

        public void Delete(AlertViewModel alert)
        {
            if (_alertWindows.TryRemove(alert.Id, out var alertWindow)) alertWindow.Close();
            Alerts.Remove(alert);
            SortByDeadlineAndActiveState(Alerts);
            Save();
            Align();
        }

        public void Save()
        {
            var alerts = new List<Alert>();
            foreach (var alert in Alerts)
            {
                alerts.Add(alert.ToAlert());
            }

            var directoryPath = _userSettings.Current.AlertsPath;
            try
            {
                var filePath = Path.Combine(directoryPath, "Alerts.json");
                if (Directory.Exists(directoryPath))
                {
                    File.WriteAllText(filePath, JsonSerializer.Serialize(alerts, _jsonSerializerOptions));
                }
                else
                {
                    _logger.LogWarning("Failed to save alerts to {filePath}, directory not exists, try create", filePath);
                    Directory.CreateDirectory(directoryPath);
                    File.WriteAllText(filePath, JsonSerializer.Serialize(alerts, _jsonSerializerOptions));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to save alerts to {directoryPath}: {exception}, please check your setting", directoryPath, e.Message);
            }
        }

        public void Align()
        {
            var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
            var screenHeight = (int)SystemParameters.PrimaryScreenHeight;
            var horizontalSpacing = _userSettings.Current.HorizontalSpacing;
            var verticalSpacing = _userSettings.Current.VerticalSpacing;
            int columns = 1;
            var nextHeight = verticalSpacing;

            SortByDeadlineAndActiveState(Alerts);
            foreach (var alert in Alerts)
            {
                if (nextHeight + (int)alert.Height > screenHeight)
                {
                    columns++;
                    nextHeight = verticalSpacing;
                }
                _alertWindows[alert.Id].Left = screenWidth - columns * (horizontalSpacing + _alertWindows[alert.Id].ActualWidth);
                _alertWindows[alert.Id].Top = nextHeight;
                nextHeight += verticalSpacing + _alertWindows[alert.Id].ActualHeight;
            }
        }

        public static void SortByDeadlineAndActiveState(List<Alert> alerts)
        {
            alerts.Sort((a, b) =>
            {
                if (a.AlertVisible && !b.AlertVisible) return -1;
                if (!a.AlertVisible && b.AlertVisible) return 1;
                return a.Deadline.CompareTo(b.Deadline);
            });
        }

        public static void SortByDeadlineAndActiveState(ObservableCollection<AlertViewModel> alerts)
        {
            List<AlertViewModel> sortedList = [.. alerts.OrderBy(a => !a.IsActive).ThenBy(a => a.Deadline)];
            for (int i = 0; i < sortedList.Count; i++)
            {
                if (alerts.IndexOf(sortedList[i]) != i)
                {
                    alerts.Move(alerts.IndexOf(sortedList[i]), i);
                }
            }
        }
    }
}
