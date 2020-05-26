using ICD10SearchService.Models;
using System.Collections.Generic;
using System.Text.Json;

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
