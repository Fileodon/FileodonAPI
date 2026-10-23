using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Models
{
    public partial class AuditLog
    {
        public int Id { get; set; }

        public string TableName { get; set; } = null!;

        public int RecordId { get; set; }

        public string Action { get; set; } = null!;

        public string? OldData { get; set; }

        public string? NewData { get; set; }

        public string? ChangedBy { get; set; }

        public DateTime? ChangedAt { get; set; }

        public virtual User? ChangedByNavigation { get; set; }
    }
}
