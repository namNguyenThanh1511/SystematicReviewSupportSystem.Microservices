using System.Text.Json;

namespace SRSS.Project.Domain.Entities
{
    public class ProjectAuditLog
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; } // e.g., "Created Project", "Updated Inclusion Criteria", etc.
        public string Description { get; set; }
        public JsonDocument? OldValue { get; set; } // JSON representation of the old value
        public JsonDocument? NewValue { get; set; } // JSON representation of the new value
        public DateTimeOffset PerformedAt { get; set; }

        //Navigation Properties
        public Project Project { get; set; }
        /*
         Transparency
         Traceability
         */
    }
}
