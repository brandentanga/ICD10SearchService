using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ICD10SearchService.Models.Icd10CM;

namespace ICD10SearchService.Models
{
    public class SearchResult
    {
        //public Diagnosis Diagnosis { get; set; }
        public ResultDiagnosis Diagnosis { get; set; }
        public decimal RelevanceScore { get; set; }
    }
}
