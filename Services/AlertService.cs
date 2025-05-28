using HandyControl.Tools.Extension;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StickyAlerts.DataAccess;
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
        public void SetCurrentUser(Guid userId);
    }

    public class AlertService : IAlertService
    {
        private List<Alert> _alerts;
        private ConcurrentDictionary<Guid, AlertWindow> _alertWindows;
        private ISettingsService<UserSettings> _userSettings;
        private ILogger<AlertService> _logger;
        private Guid? _currentUserId; // 添加当前用户ID字段
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
            InitializeDatabase();
        }

        public void SetCurrentUser(Guid userId)
        {
            _currentUserId = userId;
            Load(); // 加载该用户的便签
        }

        private void InitializeDatabase()
        {
            try
            {
                using (var connection = DatabaseHelper.GetConnection())
                {
                    connection.Open();
                    Console.WriteLine("数据库连接成功！");
                    var command = connection.CreateCommand();
                    command.CommandText = @"
             CREATE TABLE Alerts (
                 Id TEXT PRIMARY KEY,
                 Username TEXT NOT NULL,
                Title TEXT,
                Note TEXT,
                Deadline TEXT NOT NULL,
                LastModified TEXT DEFAULT CURRENT_TIMESTAMP,
                AlertVisible INTEGER DEFAULT 1,
                NoteVisible INTEGER DEFAULT 1,
                CountdownVisible INTEGER DEFAULT 1,
                Width REAL DEFAULT 300,
                Height REAL DEFAULT 200,
                Left REAL DEFAULT 100,
                Top REAL DEFAULT 100,
                Topmost INTEGER DEFAULT 0,
                Theme TEXT DEFAULT 'Default',
                FOREIGN KEY (Username) REFERENCES Users(Username) ON DELETE CASCADE
            );
                ";
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize database");
            }
        }


        public void Load()
        {
            if (!_currentUserId.HasValue)
            {
                _logger.LogWarning("No current user set, cannot load alerts");
                return;
            }

            try
            {
                // 从数据库加载
                LoadFromDatabase();

                // 保留原有的本地文件加载作为备份
                //暂不考虑
                //LoadFromLocalFile();

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load alerts");
            }
            /*
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
            */
        }
        private void LoadFromDatabase()
        {
            _alerts.Clear();

            if (!_currentUserId.HasValue) return;

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();

                command.CommandText = "SELECT * FROM Alerts WHERE Username = $username ORDER BY Deadline";

                command.Parameters.AddWithValue("$username", _currentUserId.Value.ToString());

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            _alerts.Add(new Alert
                            {
                                Id = Guid.Parse(reader["Id"].ToString()),
                                Title = reader["Title"]?.ToString(),
                                Note = reader["Note"]?.ToString(),
                                Deadline = DateTime.Parse(reader["Deadline"].ToString()),
                                LastModified = DateTime.Parse(reader["LastModified"].ToString()),
                                AlertVisible = reader["AlertVisible"] as int? == 1,  
                                NoteVisible = reader["NoteVisible"] as int? == 1,
                                CountdownVisible = reader["CountdownVisible"] as int? == 1,
                                Width = reader["Width"] as double? ?? 300,  // 使用默认值
                                Height = reader["Height"] as double? ?? 200,
                                Left = reader["Left"] as double? ?? 100,
                                Top = reader["Top"] as double? ?? 100,
                                Topmost = reader["Topmost"] as int? == 1,
                                Theme = reader["Theme"]?.ToString() ?? "Default"
                            });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error parsing alert data for ID {Id}", reader["Id"]?.ToString());
                        }
                    }
                }
            }
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
            SaveToDatabase();
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
            AddAlertWindow(vm);
            SaveToDatabase();
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
            /*
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
            */
            // 保存到数据库
            SaveToDatabase();

                // 保留本地文件保存作为备份
                //SaveToLocalFile();
    
        }

        public void SaveToDatabase()
        {

            var alertsToSave = Alerts.Select(vm => vm.ToAlert()).ToList();

            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                var fkCmd = connection.CreateCommand();
                fkCmd.CommandText = "PRAGMA foreign_keys = OFF";
                fkCmd .ExecuteNonQuery();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 插入新记录
                        foreach (var alert in alertsToSave)
                        {
                            var upsertCmd = connection.CreateCommand();
                            upsertCmd.CommandText = @"
                        INSERT  OR REPLACE INTO Alerts (
                            Id, Username, Title, Note, Deadline, LastModified, 
                            AlertVisible, NoteVisible, CountdownVisible,
                            Width, Height, Left, Top, Topmost, Theme
                        ) VALUES (
                            $id, $username, $title, $note, $deadline, $lastModified,
                            $alertVisible, $noteVisible, $countdownVisible,
                            $width, $height, $left, $top, $topmost, $theme
                        )";

                            upsertCmd.Parameters.AddWithValue("$id", alert.Id.ToString());
                            upsertCmd.Parameters.AddWithValue("$username", alert.Username.ToString());  
                            upsertCmd.Parameters.AddWithValue("$title", alert.Title ?? "");
                            upsertCmd.Parameters.AddWithValue("$note", alert.Note ?? "");
                            upsertCmd.Parameters.AddWithValue("$deadline", alert.Deadline.ToString("O"));
                            upsertCmd.Parameters.AddWithValue("$lastModified", DateTime.Now.ToString("O"));
                            upsertCmd.Parameters.AddWithValue("$alertVisible", alert.AlertVisible ? 1 : 0);
                            upsertCmd.Parameters.AddWithValue("$noteVisible", alert.NoteVisible ? 1 : 0);
                            upsertCmd.Parameters.AddWithValue("$countdownVisible", alert.CountdownVisible ? 1 : 0);
                            upsertCmd.Parameters.AddWithValue("$width", alert.Width);
                            upsertCmd.Parameters.AddWithValue("$height", alert.Height);
                            upsertCmd.Parameters.AddWithValue("$left", alert.Left);
                            upsertCmd.Parameters.AddWithValue("$top", alert.Top);
                            upsertCmd.Parameters.AddWithValue("$topmost", alert.Topmost ? 1 : 0);
                            upsertCmd.Parameters.AddWithValue("$theme", alert.Theme ?? "Default");

                            upsertCmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        _logger.LogInformation($"Successfully saved {alertsToSave.Count} alerts to database");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError(ex, "Failed to save alerts to database");
                        throw;
                    }
                }
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
