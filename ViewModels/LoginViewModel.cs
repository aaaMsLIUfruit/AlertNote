using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using StickyAlerts.DataAccess;
using StickyAlerts.Models;
using StickyAlerts.Services;
using StickyAlerts.Views;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace StickyAlerts.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _registerUsername = string.Empty;

        [ObservableProperty]
        private string _registerPassword = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsRegisterViewVisible))]
        private bool _isLoginViewVisible = true;

        public bool IsRegisterViewVisible => !IsLoginViewVisible;

        [RelayCommand]

        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                HandyControl.Controls.MessageBox.Error("请输入用户名和密码", "错误");
                return;
            }

            try
            {
                //Guid? userId = null;
                var loginSuccess = await Task.Run(() =>
                {
                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        connection.Open();

                        var cmd = connection.CreateCommand();
                        cmd.CommandText = @"
                    SELECT PasswordHash, Salt 
                    FROM Users 
                    WHERE Username = $username";
                        cmd.Parameters.AddWithValue("$username", Username);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read()) return false;
                            //userId = GetUserIdByUsername(Username); // 获取用户ID
                            var storedHash = reader["PasswordHash"].ToString();
                            var salt = reader["Salt"].ToString();
                            var inputHash = ComputePasswordHash(Password, salt);

                            return storedHash == inputHash;
                        }
                    }
                });

                if (!loginSuccess)
                {
                    HandyControl.Controls.MessageBox.Error("用户名或密码错误", "登录失败");
                    return;
                }
                // 获取AlertService实例并设置当前用户
                var alertService = App.Host.Services.GetRequiredService<IAlertService>();
               // alertService.SetCurrentUser(userId.Value); // 这里调用设置当前用户
                var shellWindow = App.Host.Services.GetRequiredService<ShellWindow>();
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Application.Current.MainWindow = shellWindow;
                    shellWindow.Show();

                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is LoginWindow loginWindow)
                        {
                            loginWindow.Close();
                            break;
                        }
                    }
                });
            }
            catch (SqliteException ex)
            {
                HandyControl.Controls.MessageBox.Error($"数据库错误: {ex.Message}", "错误");
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"登录失败: {ex.Message}", "错误");
            }
        }

        private Guid? GetUserIdByUsername(object username)
        {
            using (var connection = DatabaseHelper.GetConnection())
            {
                connection.Open();

                var cmd = connection.CreateCommand();
                cmd.CommandText = @"
        SELECT Id 
        FROM Users 
        WHERE Username = $username";
                cmd.Parameters.AddWithValue("$username", username);

                var result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException($"找不到用户名为 {username} 的用户");
                }

                var userId = Guid.Parse(result.ToString());
                return userId;
            }
        }

        [RelayCommand]
        private async Task Register()
        {
            if (string.IsNullOrWhiteSpace(RegisterUsername) ||
                string.IsNullOrWhiteSpace(RegisterPassword) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                HandyControl.Controls.MessageBox.Error("请填写完整的注册信息", "错误");
                return;
            }

            if (RegisterPassword != ConfirmPassword)
            {
                HandyControl.Controls.MessageBox.Error("两次输入的密码不一致", "错误");
                return;
            }

            try
            {
                if (RegisterPassword.Length < 8 || !RegisterPassword.Any(char.IsDigit) || !RegisterPassword.Any(char.IsLetter))
                {
                    HandyControl.Controls.MessageBox.Error("密码需至少8位且包含字母和数字", "错误");
                    return;
                }

                var success = await Task.Run(() =>
                {
                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        connection.Open();

                        var checkCmd = connection.CreateCommand();
                        checkCmd.CommandText = "SELECT COUNT(*) FROM Users WHERE Username = $username";
                        checkCmd.Parameters.AddWithValue("$username", RegisterUsername);
                        var exists = (long)checkCmd.ExecuteScalar() > 0;
                        if (exists) return false;

                        var salt = GenerateSalt();
                        var hash = ComputePasswordHash(RegisterPassword, salt);

                        var insertCmd = connection.CreateCommand();
                        insertCmd.CommandText = @"
                    INSERT INTO Users (Username, PasswordHash, Salt)
                    VALUES ($username, $hash, $salt)";
                        insertCmd.Parameters.AddWithValue("$username", RegisterUsername);
                        insertCmd.Parameters.AddWithValue("$hash", hash);
                        insertCmd.Parameters.AddWithValue("$salt", salt);

                        return insertCmd.ExecuteNonQuery() > 0;
                    }
                });

                if (!success)
                {
                    HandyControl.Controls.MessageBox.Error("用户名已存在", "注册失败");
                    return;
                }

                HandyControl.Controls.MessageBox.Success("注册成功", "提示");
                SwitchToLoginView();
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
            {
                HandyControl.Controls.MessageBox.Error("用户名已存在", "注册失败");
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error($"注册失败: {ex.Message}", "错误");
            }
        }

        private static string GenerateSalt()
        {
            var bytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private static string ComputePasswordHash(string password, string salt)
        {
            using var sha256 = SHA256.Create();
            var saltedPassword = password + salt;
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            return Convert.ToBase64String(bytes);
        }

        [RelayCommand]
        private void SwitchToRegisterView()
        {
            IsLoginViewVisible = false;
            RegisterUsername = string.Empty;
            RegisterPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }

        [RelayCommand]
        private void SwitchToLoginView()
        {
            IsLoginViewVisible = true;
            Username = string.Empty;
            Password = string.Empty;
        }
    }
}