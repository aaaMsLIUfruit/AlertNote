using StickyAlerts.ViewModels;
using System.Windows.Controls;
using System.Windows;

namespace StickyAlerts.Views
{
    /// <summary>
    /// AlertsView.xaml 的交互逻辑
    /// </summary>
    public partial class AlertsView : UserControl
    {
        public AlertsView()
        {
            InitializeComponent();
        }

        private void ComboBox_Theme_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                string theme = comboBox.Text.Trim();
                if (theme.Length > 5)
                {
                    System.Windows.MessageBox.Show("主题不能超过5个字！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    comboBox.Text = string.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(theme) && !StickyAlerts.ThemeManager.ThemeList.Contains(theme))
                {
                    StickyAlerts.ThemeManager.AddTheme(theme);
                }
            }
        }

        private void ComboBox_Theme_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && sender is ComboBox comboBox)
            {
                string theme = comboBox.Text.Trim();
                if (theme.Length > 5)
                {
                    System.Windows.MessageBox.Show("主题不能超过5个字！", "提示", MessageBoxButton.OK, MessageBoxImage.Warning);
                    comboBox.Text = string.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(theme) && !StickyAlerts.ThemeManager.ThemeList.Contains(theme))
                {
                    StickyAlerts.ThemeManager.AddTheme(theme);
                }
            }
        }

        private void AddRemarkButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取当前便签ViewModel
            if (sender is Button button && button.DataContext is StickyAlerts.ViewModels.AlertViewModel vm)
            {
                var dialog = new RemarkDialog(vm.Note ?? "");
                if (dialog.ShowDialog() == true)
                {
                    vm.Note = dialog.Remark;
                }
            }
        }
    }
}
