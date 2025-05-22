using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using StickyAlerts.Views;
using System;
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
                // TODO: 实现实际的登录逻辑
                await Task.Delay(1000);

                // 创建并显示主窗口
                var shellWindow = App.Host.Services.GetRequiredService<ShellWindow>();
                Application.Current.MainWindow = shellWindow;
                shellWindow.Show();

                // 关闭登录窗口
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is LoginWindow)
                    {
                        window.Close();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error(ex.Message, "错误");
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
                // TODO: 实现实际的注册逻辑
                await Task.Delay(1000);
                HandyControl.Controls.MessageBox.Success("注册成功", "提示");
                SwitchToLoginView();
            }
            catch (Exception ex)
            {
                HandyControl.Controls.MessageBox.Error(ex.Message, "错误");
            }
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