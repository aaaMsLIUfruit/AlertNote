using System.Collections.ObjectModel;

namespace StickyAlerts
{
    public static class ThemeManager
    {
        public static ObservableCollection<string> ThemeList { get; } = new ObservableCollection<string>();

        public static void AddTheme(string theme)
        {
            if (!string.IsNullOrWhiteSpace(theme) && !ThemeList.Contains(theme))
            {
                ThemeList.Add(theme);
            }
        }
    }
} 