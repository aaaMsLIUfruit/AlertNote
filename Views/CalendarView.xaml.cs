using System.Windows.Controls;
using System.Windows;

namespace StickyAlerts.Views
{
    public partial class CalendarView : UserControl
    {
        public CalendarView()
        {
            InitializeComponent();
        }

        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is Calendar calendar && calendar.SelectedDate.HasValue)
            {
                var viewModel = DataContext as ViewModels.CalendarViewModel;
                viewModel.SelectedDate = calendar.SelectedDate.Value;
            }
        }
    }
} 