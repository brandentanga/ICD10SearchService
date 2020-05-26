using ICD10SearchService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ICD10SearchService.Models.BagOfWord;
using static ICD10SearchService.Models.Icd10CM;

namespace ICD10SearchService.Generators
{
    public class BagOfWordsGenerator
    {
        public Dictionary<string, BagOfWord> Generate(Dictionary<string, Diagnosis> diagnoses)
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

        private void AddToBagOfWords(Dictionary<string, BagOfWord> bagOfWords, Dictionary<string, WordInDocument> wordsInDocument, string documentName)
        {
            foreach (KeyValuePair<string, WordInDocument> pair in wordsInDocument)
            {
                Upsert(bagOfWords, pair, documentName);
            }
        }

        private void Upsert(Dictionary<string, BagOfWord> bagOfWords, KeyValuePair<string, WordInDocument> pair, string documentName)
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

        private string[] Tokenize(Diagnosis diagnosis)
        {
            var tokenizedName = Tokenize(diagnosis.Name);
            var tokenizedDescription = Tokenize(diagnosis.Desc);

            return tokenizedName.Concat(tokenizedDescription).ToArray();
        }

        private string[] Tokenize(string str)
        {
            return str.ToLower().Split(' ').Select(s => s.TrimEnd('.', ',').ToLower()).ToArray();
        }
    }
}
