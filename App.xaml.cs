using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StickyAlerts.Models;
using StickyAlerts.Services;
using StickyAlerts.ViewModels;
using StickyAlerts.Views;
using System;
using System.Windows;
using Microsoft.Data.Sqlite;

namespace StickyAlerts
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost Host { get; private set; }

        public App()
        {
            // 初始化主机
            var builder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, configuration) =>
                {
                    configuration.SetBasePath(context.HostingEnvironment.ContentRootPath);
                    configuration.AddJsonFile("Settings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddDebug();
                    logging.AddConsole();
                })
                .ConfigureServices((context, services) =>
                {
                    // 注册服务
                    services.AddSingleton<ISettingsService<UserSettings>, UserSettingsService>();
                    services.AddSingleton<IAlertService, AlertService>();
                    services.AddSingleton<IShellService, ShellService>();
                    services.AddSingleton<ISystemThemeService, SystemThemeService>();
                    services.AddSingleton<IAuthService, AuthService>();
                    // 注册视图模型
                    services.AddTransient<AlertsViewModel>();
                    services.AddTransient<SettingsViewModel>();
                    services.AddTransient<ShellViewModel>();

                    // 注册视图
                    services.AddTransient<ShellWindow>();

                    // 添加配置类
                    services.Configure<UserSettings>(context.Configuration.GetSection("UserSettings"));
                });
            Host = builder.Build();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var loginWindow = new Views.LoginWindow();
            loginWindow.Show();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 初始化数据库（确保DataAccess命名空间正确）
            InitializeDatabase();

            // 其他启动逻辑（如主题加载等）

        }

        private void InitializeDatabase()
        {
            try
            {
                // 这会自动创建数据库文件和表结构
                using (var connection = DataAccess.DatabaseHelper.GetConnection())
                {
                    // 可选的测试查询（验证连接）
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
                    var tables = command.ExecuteScalar();
                    System.Diagnostics.Debug.WriteLine($"数据库初始化成功，检测到表：{tables}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"数据库初始化失败：{ex.Message}", "严重错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(); // 终止应用
            }
        }
        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // 关闭主机
            Host.Dispose();
            await Host.StopAsync();
        }
    }
}
