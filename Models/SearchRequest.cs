using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
