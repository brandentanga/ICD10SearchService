using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ICD10SearchService.Formatters;
using ICD10SearchService.Ingestion;
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

            var engine = new Engine();
            engine.Execute();
            var results = engine.Search(searchRequest.Terms);
            
            return OutputFormatter.ToJson(results);
        }
    }
}
