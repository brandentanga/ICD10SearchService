using System.Collections.Generic;

namespace ICD10SearchService.Models
{
    public class SearchRequest
    {
        public List<string> Terms { get; set; }
        public SearchRequest()
        {
            Terms = new List<string>();
        }
    }
}
