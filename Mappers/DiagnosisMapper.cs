using ICD10SearchService.Models;
using static ICD10SearchService.Models.Icd10CM;

namespace ICD10SearchService.Mappers
{
    public static class DiagnosisMapper
    {
        public static ResultDiagnosis Map(Diagnosis diagnosis)
        {
            return new ResultDiagnosis()
            {
                Name = diagnosis.Name,
                Desc = diagnosis.Desc
            };
        }
    }
}
