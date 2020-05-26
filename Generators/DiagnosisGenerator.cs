using ICD10SearchService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ICD10SearchService.Models.Icd10CM;

namespace ICD10SearchService.Generators
{
    public class DiagnosisGenerator
    {
        public Dictionary<string, Diagnosis> Generate(Icd10CM icds)
        {
            var diagnoses = new Dictionary<string, Diagnosis>();

            foreach (Chapter chapter in icds.Chapters)
            {
                foreach (Section section in chapter.Sections)
                {
                    foreach (Diagnosis diagnosis in section.Diagnoses)
                    {
                        foreach (Diagnosis innerDiagnosis in diagnosis.Diagnoses)
                        {
                            diagnoses.Add(innerDiagnosis.Name, innerDiagnosis);
                        }
                    }
                }
            }

            return diagnoses;
        }
    }
}
