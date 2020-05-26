using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ICD10SearchService.Formatters;
using ICD10SearchService.Generators;
using ICD10SearchService.Engines;
using ICD10SearchService.Mappers;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ICD10SearchService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ICD10SearchController : ControllerBase
    {
        // GET: api/<ICD10SearchController>
        [HttpGet]
        public string Get()
        {
            var query = Request.Query;
            var searchRequest = SearchRequestMapper.Map(query);

            var ingestion = new IngestionEngine();
            ingestion.Execute(new DiagnosisGenerator(), new BagOfWordsGenerator());
            var search = new SearchEngine(ingestion.BagOfWords, ingestion.Diagnoses);
            var results = search.Search(searchRequest.Terms);
            
            return OutputFormatter.ToJson(results);
        }
    }
}
