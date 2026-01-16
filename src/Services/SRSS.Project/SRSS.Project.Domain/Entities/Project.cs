using Shared.Entities.BaseEntity;
using System.Text.Json;

namespace SRSS.Project.Domain.Entities
{
    public class Project : BaseEntity<Guid>
    {
        public string Name { get; set; } // Required
        public string Abbreviation { get; set; } // Required
        public string? Description { get; set; }
        public JsonDocument ResearchQuestions { get; set; } // postge auto map to jsonb
        /*
          RQ1: What AI techniques are commonly used in adaptive learning systems?
          RQ2: What evaluation metrics are used to measure learning effectiveness?
          RQ3: What challenges and limitations are reported?

          Dùng để : Định hướng search strategy
                    Là cơ sở thiết kế data extraction form
                    Xuất hiện trong báo cáo PRISMA / final report
         */
        public JsonDocument InclusionCriteria { get; set; } // JSON
        //Inclusion Criteria là điều kiện BẮT BUỘC để một bài nghiên cứu được đưa vào review.
        /*
         Dùng rất nhiều ở:

                        Title/Abstract screening

                        Full-text screening

         Reviewer phải dựa vào IC để quyết định Include
         VD : 
                IC1: Studies published between 2015 and 2024 ( id , order , description ) 
                IC2: Peer-reviewed journal or conference papers
                IC3: Focus on machine learning techniques
                IC4: Written in English
        VD json : 
        {
          "version": 1,
          "criteria": [
            { "id": "IC1", "description": "Studies published between 2015 and 2024" },
            { "id": "IC2", "description": "Peer-reviewed papers" },
            { "id": "IC3", "description": "Written in English" }
          ]
        }
         */
        public JsonDocument ExclusionCriteria { get; set; } // JSON
        //Exclusion Criteria là điều kiện LOẠI TRỪ.
        /*
         Nếu paper thỏa 1 EC → loại ngay, dù có thỏa IC.
         VD : 
            EC1: Non-English publications ( id , order , description ) 
            EC2: Short papers (< 4 pages)
            EC3: Grey literature (blogs, white papers)
            EC4: Duplicate studies

         */
        public ProjectPhase Phase { get; set; }
        public int CriteriaVersion { get; set; } // Track changes in criteria
        public DateTimeOffset? PhaseChangedAt { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? ExpectedEndDate { get; set; }
        public DateTimeOffset? ActualEndDate { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }

        public bool IsDeleted { get; set; } // Soft delete
        public ICollection<ProjectMember> Members { get; set; }
        public ICollection<SearchSource> SearchSources { get; set; }
    }

    public enum ProjectPhase
    {
        Draft,                      // Setup RQ, IC/EC
        SearchCompleted,            // Import + dedup done
        TitleAbstractScreening,     // Dual screening
        FullTextScreening,          // Eligibility
        DataExtraction,
        QualityAssessment,
        SynthesisAndReporting,
        Locked                      // No more changes
    }

    /*
    PostgreSQL is selected for the Project service because it provides native JSONB support, 
    which fits well with semi-structured project configuration data such as research questions
    and eligibility criteria. 
    This choice improves flexibility and aligns with a polyglot persistence strategy in a microservice architecture.
    */
}
