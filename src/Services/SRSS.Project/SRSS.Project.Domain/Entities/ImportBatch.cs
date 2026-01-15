using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.Project.Domain.Entities
{
    //execution record
    public class ImportBatch : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }
        public Guid SearchSourceId { get; set; }

        public DateTimeOffset ExecutedAt { get; set; }
        public string ExecutedSearchString { get; set; }

        public int RecordsRetrieved { get; set; }

        public string Reason { get; set; } // Initial / Additional
        public ProjectPhase PhaseAtImport { get; set; }

        // Navigation Properties
        public Project Project { get; set; }
        public SearchSource SearchSource { get; set; }
    }

}
