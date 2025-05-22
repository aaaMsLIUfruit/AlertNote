using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StickyAlerts
{
    public static class TextBoxMaxLengthBehavior
    {
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.RegisterAttached(
                "MaxLength",
                typeof(int),
                typeof(TextBoxMaxLengthBehavior),
                new PropertyMetadata(0, OnMaxLengthChanged));

        public static int GetMaxLength(DependencyObject obj) => (int)obj.GetValue(MaxLengthProperty);
        public static void SetMaxLength(DependencyObject obj, int value) => obj.SetValue(MaxLengthProperty, value);

        private static void OnMaxLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox && (int)e.NewValue > 0)
            {
                comboBox.PreviewTextInput -= ComboBox_PreviewTextInput;
                comboBox.PreviewTextInput += ComboBox_PreviewTextInput;
            }
        }

        private static void ComboBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                string currentText = comboBox.Text;
                int maxLength = GetMaxLength(comboBox);
                if (currentText.Length + e.Text.Length > maxLength)
                {
                    e.Handled = true;
                }
            }
        }
    }
} 