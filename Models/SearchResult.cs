
namespace ICD10SearchService.Models
{
    public class SearchResult
    {
        public ResultDiagnosis Diagnosis { get; set; }
        public decimal RelevanceScore { get; set; }
    }
}
