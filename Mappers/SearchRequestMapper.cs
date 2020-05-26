using ICD10SearchService.Models;
using static ICD10SearchService.Constants.Constants;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ICD10SearchService.Mappers
{
    public static class SearchRequestMapper
    {
        public static SearchRequest Map(IQueryCollection requestParameters)
        {
            var searchRequest = new SearchRequest();

            var dict = requestParameters.ToImmutableDictionary();
            if (dict.ContainsKey(Terms))
            {
                searchRequest.Terms = GetTerms(dict[Terms]);
            }

            return searchRequest;
        }

        private static List<string> GetTerms(string terms)
        {
            return terms.Split(RequestParameterSplit).ToList();
        }
    }
}
