using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StickyAlerts.Models
{
    public class AlertCollectionDeletingMessage
    {
        public Guid Id { get; }

        public AlertCollectionDeletingMessage(Guid id)
        {
            Id = id;
        }
    }
}
