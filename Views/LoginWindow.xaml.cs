using HandyControl.Controls;
using StickyAlerts.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace StickyAlerts.Views
{
    public partial class LoginWindow : HandyControl.Controls.Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;

            // 使用 HandyControl 的 PasswordChanged 事件
            LoginPasswordBox.PasswordChanged += (s, e) => _viewModel.Password = LoginPasswordBox.Password;
            RegisterPasswordBox.PasswordChanged += (s, e) => _viewModel.RegisterPassword = RegisterPasswordBox.Password;
            ConfirmPasswordBox.PasswordChanged += (s, e) => _viewModel.ConfirmPassword = ConfirmPasswordBox.Password;

            // 切换视图时清空密码框
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(LoginViewModel.IsLoginViewVisible))
                {
                    if (_viewModel.IsLoginViewVisible)
                    {
                        LoginPasswordBox.Password = string.Empty;
                    }
                    else
                    {
                        RegisterPasswordBox.Password = string.Empty;
                        ConfirmPasswordBox.Password = string.Empty;
                    }
                }
            };

            // 添加窗口关闭事件处理
            Closing += (s, e) =>
            {
                // 只有当没有其他窗口打开时才关闭应用程序
                if (Application.Current.Windows.Count <= 1)
                {
                    Application.Current.Shutdown();
                }
            };
        }
    }
} 