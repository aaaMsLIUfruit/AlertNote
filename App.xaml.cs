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

        protected override async void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // 关闭主机
            Host.Dispose();
            await Host.StopAsync();
        }
    }
}
