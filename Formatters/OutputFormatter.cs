using ICD10SearchService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ICD10SearchService.Formatters
{
    public class OutputFormatter
    {
        private static JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        public static string ToJson(List<SearchResult> searchResults)
        {
            return JsonSerializer.Serialize(searchResults, options);
        }
    }
}
