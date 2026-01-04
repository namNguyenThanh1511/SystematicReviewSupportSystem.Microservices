using Shared.Entities.BaseEntity;

namespace SRSS.Project.Domain.Entities
{
    public class ProjectStageStage : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }
        public ProjectStageName StageName { get; set; } // Import, Screening, etc.
        public ProjectStageStatus Status { get; set; } // Pending, InProgress, Completed
        public int CompletionPercentage { get; set; } 
        //Navigation Properties
        public Project Project { get; set; }

    }

    public enum ProjectStageStatus
    {
        Pending,
        InProgress,
        Completed
    }
    public enum ProjectStageName
    {
        Import,
        Screening,
        Review,
        Publish
    }

}
