using System.Windows;

namespace StickyAlerts.Views
{
    public partial class RemarkDialog : Window
    {
        public string Remark { get; private set; } = string.Empty;

        public RemarkDialog(string initialRemark = "")
        {
            InitializeComponent();
            RemarkTextBox.Text = initialRemark;
            RemarkTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Remark = RemarkTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 