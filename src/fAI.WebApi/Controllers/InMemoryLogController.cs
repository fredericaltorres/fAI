using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Caching.Memory;
using static System.Net.Mime.MediaTypeNames;

namespace fAI.WebApi.Controllers
{
    // https://portal.azure.com/#@fredericaltorreslive.onmicrosoft.com/resource/subscriptions/57646804-986c-47e8-af66-a3abec32e52a/resourceGroups/ftorres/providers/Microsoft.Web/sites/fAIWebApi/configuration
    // KUDU https://faiwebapi.scm.azurewebsites.net/Env.cshtml
    // https://faiwebapi.azurewebsites.net/Embedding


    //  curl.exe -X GET  -H "accept: text/plain" "https://localhost:7009/InMemoryLog/rows"
    //  curl.exe -X PUT  -H "accept: text/plain" "https://localhost:7009/InMemoryLog/clear"
    //  curl.exe -X POST -H "accept: */*" -H "Content-Type: application/json"  -d "\"tutu\""  "https://localhost:7009/InMemoryLog/rows"


    //  curl.exe -X GET   "https://faiwebapi.azurewebsites.net/InMemoryLog/rows"
    //  curl.exe -X PUT   "https://faiwebapi.azurewebsites.net/InMemoryLog/clear"
    //  curl.exe -X POST -H "Content-Type: application/json"   -d "\"Zizi\""  "https://faiwebapi.azurewebsites.net/InMemoryLog/rows"
    //  curl.exe -X POST -H "accept: */*" -H "Content-Type: application/json"  -d "\"Zizi\""  "https://faiwebapi.azurewebsites.net/InMemoryLog/rows"



    [ApiController]
    [Route("[controller]")]
    public class InMemoryLogController : ControllerBase
    {
        private static List<string> rows = new List<string>() { $"[{DateTime.Now}]Init" };
        private IConfiguration _configuration;

        public InMemoryLogController(IConfiguration configuration)
        {
            _configuration = configuration;
            fAI.Logger.DefaultLogFileName = Path.Combine(Path.GetTempPath(), "fAI.log");
        }

        [HttpPut("clear")]
        public void Clear()
        {
            rows.Clear();
        }

        [HttpGet("rows")]
        public IEnumerable<string> GetRows()
        {
            return rows;
        }

        [HttpPost("rows")]
        public IActionResult AddRow([FromBody] string text)
        {
            rows.Add(text);
            return Ok();
        }
    }
}