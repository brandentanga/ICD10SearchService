using ICD10SearchService.Mappers;
using ICD10SearchService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ICD10SearchService.Models.Icd10CM;

namespace ICD10SearchService.Engines
{
    public class SearchEngine
    {
        private Dictionary<string, BagOfWord> bagOfWords;
        private Dictionary<string, Diagnosis> diagnoses;
        public SearchEngine(Dictionary<string, BagOfWord> bagOfWords, Dictionary<string, Diagnosis> diagnoses)
        {
            this.diagnoses = diagnoses;
            this.bagOfWords = bagOfWords;
        }
        public List<SearchResult> Search(List<string> terms)
        {
            return TfIdfSearch(bagOfWords, diagnoses, terms);
        }

        private static List<SearchResult> TfIdfSearch(Dictionary<string, BagOfWord> bagOfWords, Dictionary<string, Diagnosis> diagnoses, List<string> inputs)
        {
            var combinedResults = new Dictionary<string, SearchResult>();

            foreach (string input in inputs)
            {
                var bagOfWord = bagOfWords[input];
                var resultsForSingleTerm = bagOfWord.SortedWordInDocuments.Aggregate(new Dictionary<string, SearchResult>(), (acc, doc) =>
                {
                    var searchResult = new SearchResult()
                    {
                        //Diagnosis = diagnoses[doc.DocumentName],
                        Diagnosis = DiagnosisMapper.Map(diagnoses[doc.DocumentName]),
                        RelevanceScore = doc.TFIDF
                    };
                    acc.Add(doc.DocumentName, searchResult);
                    return acc;
                });
                Merge(combinedResults, resultsForSingleTerm);
            }

            var results = combinedResults.Aggregate(new List<SearchResult>(), (acc, kv) =>
            {
                acc.Add(kv.Value);
                return acc;
            });
            results.Sort((a, b) => b.RelevanceScore.CompareTo(a.RelevanceScore));


            return results;
        }

        private static void Merge(Dictionary<string, SearchResult> leftResults, Dictionary<string, SearchResult> rightResults)
        {
            foreach (KeyValuePair<string, SearchResult> pair in rightResults)
            {
                if (leftResults.ContainsKey(pair.Key))
                {
                    leftResults[pair.Key].RelevanceScore += pair.Value.RelevanceScore;
                }
                else
                {
                    leftResults.Add(pair.Key, pair.Value);
                }
            }
        }
    }
}
