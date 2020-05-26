using ICD10SearchService.Generators;
using ICD10SearchService.Mappers;
using ICD10SearchService.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static ICD10SearchService.Models.BagOfWord;
using static ICD10SearchService.Models.Icd10CM;

namespace ICD10SearchService.Engines
{
    public class IngestionEngine
    {
        public Dictionary<string, BagOfWord> BagOfWords { get; private set; }
        public Dictionary<string, Diagnosis> Diagnoses { get; private set; }
        
        public void Execute(DiagnosisGenerator diagnosisGenerator, BagOfWordsGenerator bagOfWordsGenerator)
        {
            var icds = LoadIcd10(@"C:\Users\brand\Documents\Demo\ICD10\icd10cm_tabular_2020.xml");
            //NaiveSearch(icds);
            //diagnoses = GenerateDiagnoses(icds);
            Diagnoses = diagnosisGenerator.Generate(icds);
            //bagOfWords = GenerateBagOfWords(diagnoses);
            BagOfWords = bagOfWordsGenerator.Generate(Diagnoses);
            CalculateTfIdf(BagOfWords, Diagnoses);
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

        private static Icd10CM LoadIcd10(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Icd10CM), new XmlRootAttribute("ICD10CM.tabular"));
            StreamReader reader = new StreamReader(filePath);
            Icd10CM icd = (Icd10CM)serializer.Deserialize(reader);
            return icd;
        }
    }
}
