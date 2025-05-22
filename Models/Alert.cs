using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StickyAlerts.Models
{
    public class Alert
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Note { get; set; }
        public DateTime Deadline { get; set; }
        public DateTime LastModified { get; set; }
        public bool AlertVisible { get; set; }
        public bool NoteVisible { get; set; }
        public bool CountdownVisible { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public bool Topmost { get; set; }
        public string Theme { get; set; } = string.Empty;
    }
}
