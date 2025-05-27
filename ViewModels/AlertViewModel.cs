using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using HandyControl.Controls;
using Microsoft.Extensions.DependencyInjection;
using StickyAlerts.Models;
using StickyAlerts.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace StickyAlerts.ViewModels
{
    public partial class AlertViewModel : ObservableObject
    {
        [ObservableProperty]
        private Guid _id;

        [ObservableProperty]
        private string? _title;

        [ObservableProperty]
        private string? _note;

        [ObservableProperty]
        private DateTime _deadline;

        [ObservableProperty]
        private DateTime _lastModified;

        [ObservableProperty]
        private bool _alertVisible;

        [ObservableProperty]
        private bool _noteVisible;

        [ObservableProperty]
        private bool _countdownVisible;

        [ObservableProperty]
        private double _width;

        [ObservableProperty]
        private double _height;

        [ObservableProperty]
        private double _left;

        [ObservableProperty]
        private double _top;

        [ObservableProperty]
        private bool _topmost;

        [ObservableProperty]
        private string _theme = string.Empty;

        partial void OnDeadlineChanged(DateTime oldValue, DateTime newValue)
        {
            // 倒计时变化后需要处理 AlertsView 的"进行中"与"已过时"列表的更新
            WeakReferenceMessenger.Default.Send(new AlertPropertyChangedMessage(Id, nameof(Deadline)));
        }

        partial void OnAlertVisibleChanged(bool oldValue, bool newValue)
        {
            return;
            var alertService = App.Host.Services.GetRequiredService<IAlertService>();
            alertService.Align();
        }

        public TimeSpan Remaining => IsActive ? Deadline - DateTime.Now : TimeSpan.Zero;

        public bool IsActive => Deadline >= DateTime.Now;

        public AlertViewModel(Alert alert)
        {
            Id = alert.Id;
            Title = alert.Title;
            Note = alert.Note;
            Deadline = alert.Deadline;
            LastModified = alert.LastModified;
            AlertVisible = alert.AlertVisible;
            NoteVisible = alert.NoteVisible;
            CountdownVisible = alert.CountdownVisible;
            Width = alert.Width;
            Height = alert.Height;
            Left = alert.Left;
            Top = alert.Top;
            Topmost = alert.Topmost;
            Theme = alert.Theme;
        }

        [RelayCommand]
        public void ShowShell()
        {

        }

        [RelayCommand]
        public void Delete()
        {
            var result = System.Windows.MessageBox.Show(
                "确定要删除这个便签吗？",
                "删除确认",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
        {
            var alertService = App.Host.Services.GetRequiredService<IAlertService>();
            alertService.Delete(Id);
            }
        }

        [RelayCommand]
        public void Algin()
        {
            var alertService = App.Host.Services.GetRequiredService<IAlertService>();
            alertService.Align();
        }

        [RelayCommand]
        public void Exit()
        {
            App.Current.Shutdown();
        }

        public Alert ToAlert()
        {
            return new Alert()
            {
                Id = Id,
                Title = Title,
                Note = Note,
                Deadline = Deadline,
                LastModified = LastModified,
                AlertVisible = AlertVisible,
                NoteVisible = NoteVisible,
                CountdownVisible = CountdownVisible,
                Width = Width,
                Height = Height,
                Left = Left,
                Top = Top,
                Topmost = Topmost,
                Theme = Theme,
            };
        }
    }
}
