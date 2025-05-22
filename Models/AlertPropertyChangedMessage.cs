using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StickyAlerts.Models
{
    public class AlertPropertyChangedMessage
    {
        public Guid Id { get; set; }
        public string PropertyName { get; set; }
        public Type? ValueType { get; set; }
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        public AlertPropertyChangedMessage(Guid id, string name, Type? type = null, object? oldValue = null, object? newValue = null)
        {
            Id = id;
            PropertyName = name;
            ValueType = type;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
