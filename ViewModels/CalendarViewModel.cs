using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using StickyAlerts.Services;
using StickyAlerts.ViewModels;

namespace StickyAlerts.ViewModels
{
    public class CalendarViewModel : ObservableObject
    {
        public ObservableCollection<AlertViewModel> AllAlerts { get; }
        public ObservableCollection<AlertViewModel> FilteredAlerts { get; } = new();

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                SetProperty(ref _selectedDate, value);
                FilterAlerts();
            }
        }

        public CalendarViewModel()
        {
            // 获取全局便签列表
            var alertService = App.Host.Services.GetService(typeof(IAlertService)) as IAlertService;
            AllAlerts = alertService?.Alerts ?? new ObservableCollection<AlertViewModel>();
            FilterAlerts();
        }

        private void FilterAlerts()
        {
            FilteredAlerts.Clear();
            foreach (var alert in AllAlerts.Where(a => a.Deadline.Date == SelectedDate.Date))
            {
                FilteredAlerts.Add(alert);
            }
        }
    }
} 