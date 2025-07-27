using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using static System.Net.Mime.MediaTypeNames;

namespace fAI.WebApi.Controllers
{
    // https://portal.azure.com/#@fredericaltorreslive.onmicrosoft.com/resource/subscriptions/57646804-986c-47e8-af66-a3abec32e52a/resourceGroups/ftorres/providers/Microsoft.Web/sites/fAIWebApi/configuration
    // KUDU https://faiwebapi.scm.azurewebsites.net/Env.cshtml
    // https://faiwebapi.azurewebsites.net/Embedding

    [ApiController]
    [Route("[controller]")]
    public class JsonCloudDBController : ControllerBase
    {
        private IConfiguration _configuration;

        public JsonCloudDBController(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            fAI.Logger.DefaultLogFileName = Path.Combine(Path.GetTempPath(), "fAI.log");
        }

        // curl.exe -X POST -H "Content-type: application/json" -d "{""id"": ""sample-checklist"",""title"": ""Sample Checklist"",""items"": [{""id"": ""1"",""title"": ""Item 1"",""imageUrl"": ""https://example.com/image1.jpg"",""completed"": false    }]}"  "https://faiwebapi.azurewebsites.net/jsonclouddb?filename=tata"
        // curl.exe -X POST -H "Content-type: application/json" -d "{""id"": ""sample-checklist"",""title"": ""Sample Checklist"",""items"": [{""id"": ""1"",""title"": ""Item 1"",""imageUrl"": ""https://example.com/image1.jpg"",""completed"": false    }]}"  "https://localhost:7009/jsonclouddb?filename=titi"


        [HttpGet(Name = "GetJsonDb")]
        public CheckList GetJsonDb([FromQuery] string filename)
        {
            return new FredisDB().GetCheckList(filename);
        }


        // curl.exe -X GET -H "Content-type: application/json"   "https://faiwebapi.azurewebsites.net/jsonclouddb?filename=titi"
        // curl.exe -X GET -H "Content-type: application/json"   "https://localhost:7009/jsonclouddb?filename=titi"
        [HttpPost(Name = "SetJsonDb")]
        public bool SetJsonDb([FromQuery]string filename, [FromBody] CheckList checkList)
        {
            new FredisDB().AddUpdateCheckList(filename, checkList);
            return true;
        }

        // curl.exe -X DELETE "https://faiwebapi.azurewebsites.net/JsonCloudDB?filename=titi&type=checklist" -H "accept: text/plain" -H "Content-Type: application/json"
        [HttpDelete(Name = "DeleteJsonDb")]
        public bool DeleteJsonDb([FromQuery] string filename, [FromQuery] string type = "checklist")
        {
            var f = new FredisDB();
            f.DeleteCheckList(filename);
            return true;
        }
    }
}