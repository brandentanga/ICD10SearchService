using ICD10SearchService.Mappers;
using ICD10SearchService.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static ICD10SearchService.Models.BagOfWord;
using static ICD10SearchService.Models.Icd10CM;

namespace ICD10SearchService.Ingestion
{
    public class Engine
    {
        private Dictionary<string, BagOfWord> bagOfWords;
        private Dictionary<string, Diagnosis> diagnoses;
        public List<SearchResult> Search(List<string> terms)
        {
            return TfIdfSearch(bagOfWords, diagnoses, terms);
        }
        public void Execute()
        {
            var icds = LoadIcd10(@"C:\Users\brand\Documents\Demo\ICD10\icd10cm_tabular_2020.xml");
            //NaiveSearch(icds);
            diagnoses = GenerateDiagnoses(icds);
            bagOfWords = GenerateBagOfWords(diagnoses);
            CalculateTfIdf(bagOfWords, diagnoses);
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

        private static void CalculateTfIdf(Dictionary<string, BagOfWord> bagOfWords, Dictionary<string, Diagnosis> diagnoses)
        {
            foreach (KeyValuePair<string, BagOfWord> pair in bagOfWords)
            {
                var bagOfWord = pair.Value;
                CalculateIdf(bagOfWord, diagnoses.Count);
                CalculateTf(bagOfWord, diagnoses);
                CalculateTfIdf(bagOfWord);
                SortByTfIdf(bagOfWord);
            }
        }

        private static void SortByTfIdf(BagOfWord bagOfWord)
        {
            var documents = bagOfWord.WordInDocuments;
            var sortedDocuments = bagOfWord.SortedWordInDocuments;
            sortedDocuments = documents.Aggregate(sortedDocuments, (acc, pair) =>
            {
                acc.Add(pair.Value);
                return acc;
            });
            sortedDocuments.Sort((a, b) => b.TFIDF.CompareTo(a.TFIDF));
        }

        private static void CalculateTfIdf(BagOfWord bagOfWord)
        {
            foreach (KeyValuePair<string, WordInDocument> pair in bagOfWord.WordInDocuments)
            {
                var wordInDocument = pair.Value;
                wordInDocument.TFIDF = wordInDocument.TF * bagOfWord.IDF;
            }
        }

        private static void CalculateTf(BagOfWord bagOfWord, Dictionary<string, Diagnosis> diagnoses)
        {
            foreach (KeyValuePair<string, WordInDocument> pair in bagOfWord.WordInDocuments)
            {
                var diagnosis = diagnoses[pair.Key];
                decimal totalWordCountInDocument = diagnosis.Desc.Split(' ').Count() + diagnosis.Name.Split(' ').Count();
                var wordInDocument = pair.Value;
                wordInDocument.TF = wordInDocument.TotalCountInDocument / totalWordCountInDocument;
            }
        }

        private static void CalculateIdf(BagOfWord bagOfWord, int numberOfDocuments)
        {
            var dividedCount = numberOfDocuments / bagOfWord.WordInDocuments.Count;
            bagOfWord.IDF = (decimal)Math.Log10(dividedCount);
        }

        private static Dictionary<string, BagOfWord> GenerateBagOfWords(Dictionary<string, Diagnosis> diagnoses)
        {
            var dict = new Dictionary<string, BagOfWord>();

            // Tokenize a document <-- dictionary of key = word, value = count
            // Add the tokens to the bag of words
            foreach (KeyValuePair<string, Diagnosis> pair in diagnoses)
            {
                var tokenizedDocument = Tokenize(pair.Value);
                var wordsInDocument = DedupeAndGenerateCount(tokenizedDocument, pair.Value.Name);
                AddToBagOfWords(dict, wordsInDocument, pair.Value.Name);
            }

            return dict;
        }

        private static void AddToBagOfWords(Dictionary<string, BagOfWord> bagOfWords, Dictionary<string, WordInDocument> wordsInDocument, string documentName)
        {
            foreach (KeyValuePair<string, WordInDocument> pair in wordsInDocument)
            {
                Upsert(bagOfWords, pair, documentName);
            }
        }

        private static void Upsert(Dictionary<string, BagOfWord> bagOfWords, KeyValuePair<string, WordInDocument> pair, string documentName)
        {
            if (bagOfWords.ContainsKey(pair.Key))
            {
                bagOfWords[pair.Key].WordInDocuments.Add(documentName, pair.Value);
            }
            else
            {
                var bagOfWord = new BagOfWord(pair.Key, documentName, pair.Value);
                bagOfWords.Add(pair.Key, bagOfWord);
            }
        }

        private static Dictionary<string, WordInDocument> DedupeAndGenerateCount(string[] tokens, string documentName)
        {
            Array.Sort(tokens);
            var wordsInDocument = new Dictionary<string, WordInDocument>();
            foreach (string token in tokens)
            {
                Upsert(wordsInDocument, token, documentName);
            }

            return wordsInDocument;
        }

        private static void Upsert(Dictionary<string, WordInDocument> dict, string token, string documentName)
        {
            if (dict.ContainsKey(token))
            {
                dict[token].TotalCountInDocument += 1;
            }
            else
            {
                dict.Add(token, new WordInDocument()
                {
                    DocumentName = documentName,
                    TotalCountInDocument = 1
                });
            }
        }

        private static string[] Tokenize(Diagnosis diagnosis)
        {
            var tokenizedName = Tokenize(diagnosis.Name);
            var tokenizedDescription = Tokenize(diagnosis.Desc);

            return tokenizedName.Concat(tokenizedDescription).ToArray();
        }

        private static string[] Tokenize(string str)
        {
            return str.ToLower().Split(' ').Select(s => s.TrimEnd('.', ',').ToLower()).ToArray();
        }

        private static Icd10CM LoadIcd10(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Icd10CM), new XmlRootAttribute("ICD10CM.tabular"));
            StreamReader reader = new StreamReader(filePath);
            Icd10CM icd = (Icd10CM)serializer.Deserialize(reader);
            return icd;
        }

        private static Dictionary<string, Diagnosis> GenerateDiagnoses(Icd10CM icds)
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
