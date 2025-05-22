using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StickyAlerts.Controls
{
    public class PathPicker : Control
    {
        static PathPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PathPicker), new FrameworkPropertyMetadata(typeof(PathPicker)));
        }

        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register("SelectedPath", typeof(string), typeof(PathPicker), new PropertyMetadata(string.Empty));

        public string SelectedPath
        {
            get { return (string)GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(PathPicker), new PropertyMetadata(false));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Button browseButton = GetTemplateChild("PART_BrowseButton") as Button;
            if (browseButton != null)
            {
                browseButton.Click += BrowseButton_Click;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var openFolderDialog = new OpenFolderDialog();
            openFolderDialog.ValidateNames = false;
            openFolderDialog.Multiselect = false;
            //openFolderDialog.DefaultDirectory = SelectedPath;
            openFolderDialog.FolderName = "Folder Selection";
            if (openFolderDialog.ShowDialog() == true)
            {
                SelectedPath = openFolderDialog.FolderName;
            }
        }
    }
}
