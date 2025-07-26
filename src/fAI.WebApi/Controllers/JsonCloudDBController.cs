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

        [HttpGet(Name = "GetJsonDb")]
        public string GetJsonDb([FromQuery] string filename, [FromQuery] string type = "checklist")
        {
            var f = new FredisDB();
            var jsonText =  f.GetFileName(type, filename);
            if (string.IsNullOrEmpty(jsonText))
                return string.Empty;

            return jsonText;
        }

        [HttpPost(Name = "SetJsonDb")]
        public bool SetJsonDb([FromQuery]string filename, [FromBody] string jsonText, [FromQuery] string type = "checklist")
        {
            var f = new FredisDB();
            f.AddUpdateFileName(type, filename, jsonText);
            return true;
        }
    }
}