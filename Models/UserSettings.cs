using System.Text.Json;

namespace StickyAlerts.Models
{
    public class UserSettings
    {
        public string Language { get; set; }
        public string Theme { get; set; }
        public bool Topmost { get; set; }
        public double HorizontalSpacing { get; set; }
        public double VerticalSpacing { get; set; }
        public bool AutoStart { get; set; }
        public bool HideShell { get; set; }
        public bool ShowTrayIcon { get; set; }
        public string AlertsPath { get; set; }
    }
}
