using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using HandyControl.Controls;
using StickyAlerts.Models;
using StickyAlerts.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StickyAlerts.ViewModels
{
    public partial class AlertsViewModel : ObservableObject, IRecipient<AlertCollectionAddingMessage>, IRecipient<AlertPropertyChangedMessage>
    {
        private IAlertService _alertService;

        public ObservableCollection<AlertViewModel> Alerts { get; }
        public ObservableCollection<AlertViewModel> ActivedAlerts { get; }
        public ObservableCollection<AlertViewModel> UnactivedAlerts { get; }

        [ObservableProperty]
        private string _newTheme = string.Empty;
        [ObservableProperty]
        private string _newTitle = string.Empty;
        [ObservableProperty]
        private string _newNote = string.Empty;

        public AlertsViewModel(IAlertService alertService)
        {
            _alertService = alertService;

            Alerts = _alertService.Alerts;
            ActivedAlerts = new(Alerts.Where(a => a.IsActive));
            UnactivedAlerts = new(Alerts.Where(a => !a.IsActive));
            Alerts.CollectionChanged += Alerts_CollectionChanged;

            WeakReferenceMessenger.Default.UnregisterAll(this);
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        private void Alerts_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (AlertViewModel alert in e.NewItems)
                    {
                        if (alert.IsActive) ActivedAlerts.Add(alert);
                        else UnactivedAlerts.Add(alert);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (AlertViewModel alert in e.OldItems)
                    {
                        if (alert.IsActive) ActivedAlerts.Remove(alert);
                        else UnactivedAlerts.Remove(alert);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    AlertService.SortByDeadlineAndActiveState(ActivedAlerts);
                    AlertService.SortByDeadlineAndActiveState(UnactivedAlerts);
                    break;

                default:
                    ActivedAlerts.Clear();
                    UnactivedAlerts.Clear();
                    foreach (var alert in Alerts)
                    {
                        if (alert.IsActive) ActivedAlerts.Add(alert);
                        else UnactivedAlerts.Add(alert);
                    }
                    break;
            }
        }

        [RelayCommand]
        public void Add()
        {
            if (!string.IsNullOrWhiteSpace(NewTheme))
            {
                StickyAlerts.ThemeManager.AddTheme(NewTheme);
            }
            _alertService.Add(
                NewTitle,
                NewNote,
                DateTime.Today.AddDays(1).AddHours(9),
                NewTheme,
                true,
                true,
                false
            );
            NewTitle = string.Empty;
            NewNote = string.Empty;
            NewTheme = string.Empty;
        }

        [RelayCommand]
        public void Delete(object parameter)
        {
            AlertViewModel? alert = null;
            if (parameter is AlertViewModel alertViewModel)
            {
                alert = alertViewModel;
            }
            else if (parameter is Guid id)
            {
                alert = Alerts.FirstOrDefault(a => a.Id == id);
            }

            if (alert != null)
            {
                var result = System.Windows.MessageBox.Show(
                    "确定要删除这个便签吗？",
                    "删除确认",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _alertService.Delete(alert);
                }
            }
            else
            {
                throw new ArgumentException("Invalid parameter type.");
            }
        }

        public void Receive(AlertCollectionAddingMessage message)
        {
            _alertService.Add();
        }

        public void Receive(AlertPropertyChangedMessage message)
        {
            if (message.PropertyName == nameof(AlertViewModel.Deadline))
            {
                var alert = Alerts.FirstOrDefault(a => a.Id == message.Id);
                if (alert != null)
                {
                    if (alert.IsActive)
                    {
                        if (!ActivedAlerts.Contains(alert))
                        {
                            ActivedAlerts.Add(alert);
                        }
                        if (UnactivedAlerts.Contains(alert))
                        {
                            UnactivedAlerts.Remove(alert);
                        }
                    }
                    else
                    {
                        if (ActivedAlerts.Contains(alert))
                        {
                            ActivedAlerts.Remove(alert);
                        }
                        if (!UnactivedAlerts.Contains(alert))
                        {
                            UnactivedAlerts.Add(alert);
                        }
                    }
                }
            }
        }
    }
}
