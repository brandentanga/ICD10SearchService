using System.Collections.Generic;

namespace ICD10SearchService.Models
{
    public class BagOfWord
    {
        public string Word { get; set; }
        public int TotalCountInCorpus { get; set; }
        public decimal IDF { get; set; }
        public Dictionary<string, WordInDocument> WordInDocuments { get; }
        public List<WordInDocument> SortedWordInDocuments { get; set; }

        public BagOfWord(string word = "", string documentName = "", WordInDocument initialWordInDocument = null)
        {
            this.Word = word;
            WordInDocuments = new Dictionary<string, WordInDocument>();
            if (initialWordInDocument != null)
            {
                WordInDocuments.Add(documentName, initialWordInDocument);
            }
            SortedWordInDocuments = new List<WordInDocument>();
        }

        public class WordInDocument
        {
            public string DocumentName { get; set; }
            public int TotalCountInDocument { get; set; }
            public decimal TF { get; set; }
            public decimal TFIDF { get; set; }
        }
    }
}
