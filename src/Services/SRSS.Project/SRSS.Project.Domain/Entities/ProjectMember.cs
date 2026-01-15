using Shared.Entities.BaseEntity;

namespace SRSS.Project.Domain.Entities
{
    public class ProjectMember : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public ProjectRole Role { get; set; } // LeadReviewer, Reviewer, Viewer
        public DateTimeOffset JoinedAt { get; set; }

        //Navigation Properties
        public Project Project { get; set; }
    }
    public enum ProjectRole
    {
        LeadReviewer,
        MainResearcher,
        Reviewer
    }

}
