using Shared.Entities.BaseEntity;

namespace SRSS.Project.Domain.Entities
{
    //Search strategy
    /*
    Search Source = cam kết học thuật rằng:

    Bạn sẽ tìm ở đâu

    Bạn dự định dùng query nào

    Bạn đã tuân theo protocol

    ==> Trong PRISMA, phần này nằm ở Methods – Information Sources
     */
    public class SearchSource : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }
        public string SourceName { get; set; } // e.g.,PubMed, Scopus, Google Scholar, Web of Science, ...
        public SearchSourceType SourceType { get; set; }
        public string PlannedSearchString { get; set; }
        public string Notes { get; set; }

        //Navigation Properties
        public Project Project { get; set; }

    }

    public enum SearchSourceType
    {
        Database,
        Register,
        GreyLiterature,
        HandSearch,
        Other
    }

}
