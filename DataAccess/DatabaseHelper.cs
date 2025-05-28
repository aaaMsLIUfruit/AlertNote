using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace StickyAlerts.DataAccess
{
    public static class DatabaseHelper
    {
        private static readonly string _dbPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Data",
            "sticky_notes.db"
        );

        public static SqliteConnection GetConnection()
        {
            EnsureDatabaseDirectoryExists();
            var connection = new SqliteConnection($"Data Source={_dbPath};");
            InitializeDatabaseSchema(connection);
            return connection;
        }

        private static void EnsureDatabaseDirectoryExists()
        {
            var directory = Path.GetDirectoryName(_dbPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static void InitializeDatabaseSchema(SqliteConnection connection)
        {
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                var command = connection.CreateCommand();

                // 用户表
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Username TEXT PRIMARY KEY,
                    PasswordHash TEXT NOT NULL,
                    Salt TEXT NOT NULL,
                    CreatedTime TEXT DEFAULT CURRENT_TIMESTAMP
                )";
                command.ExecuteNonQuery();

                // 便签表
                 command.CommandText = @"
                 CREATE TABLE IF NOT EXISTS Alerts (
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
            )";
                command.ExecuteNonQuery();

                // 用户设置表
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS UserSettings (
                    Username TEXT PRIMARY KEY,
                    Language TEXT DEFAULT 'zh-CN',
                    Theme TEXT DEFAULT 'Default',
                    Topmost INTEGER DEFAULT 0,
                    HorizontalSpacing REAL DEFAULT 10,
                    VerticalSpacing REAL DEFAULT 10,
                    AutoStart INTEGER DEFAULT 0,
                    HideShell INTEGER DEFAULT 0,
                    ShowTrayIcon INTEGER DEFAULT 1,
                    AlertsPath TEXT DEFAULT '',
                    FOREIGN KEY (Username) REFERENCES Users(Username) ON DELETE CASCADE
                )";
                command.ExecuteNonQuery();

                // 创建索引
                command.CommandText = @"
                CREATE INDEX IF NOT EXISTS idx_alerts_deadline ON Alerts(Deadline);
                CREATE INDEX IF NOT EXISTS idx_alerts_modified ON Alerts(LastModified);
                CREATE INDEX IF NOT EXISTS idx_alerts_user ON Alerts(Username)";
                command.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// 计算动态优先级（基于截止时间）
        /// </summary>
        public static int CalculatePriority(DateTime deadline)
        {
            var timeLeft = deadline - DateTime.Now;
            return timeLeft.TotalHours switch
            {
                < 0 => 3,    // 过期（最高优先级）
                < 24 => 2,   // 24小时内（高优先级）
                < 72 => 1,   // 3天内（中等优先级）
                _ => 0       // 其他（普通优先级）
            };
        }

        /// <summary>
        /// 重置数据库（开发调试用）
        /// </summary>
        public static void ResetDatabase()
        {
            if (File.Exists(_dbPath))
            {
                File.Delete(_dbPath);
                InitializeDatabaseSchema(GetConnection());
            }
        }
    }
}