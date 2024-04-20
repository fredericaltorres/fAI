using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace fAI.WebApi.Controllers
{
    // https://portal.azure.com/#@fredericaltorreslive.onmicrosoft.com/resource/subscriptions/57646804-986c-47e8-af66-a3abec32e52a/resourceGroups/ftorres/providers/Microsoft.Web/sites/fAIWebApi/configuration
    // KUDU https://faiwebapi.scm.azurewebsites.net/Env.cshtml
    // https://faiwebapi.azurewebsites.net/Embedding
    [ApiController]
    [Route("[controller]")]
    public class EmbeddingController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private IConfiguration _configuration;

        public EmbeddingController(ILogger<WeatherForecastController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            fAI.Logger.DefaultLogFileName = Path.Combine(Path.GetTempPath(), "fAI.log");
        }

        [HttpGet(Name = "GetEmbedding")]
        public IEnumerable<float> GetEmbedding()
        {
            return  new List<float>() { 1f,2f,3f,4f,5f}; 
        }

        [HttpPost(Name = "ComputeEmbedding")]
        public IEnumerable<float> ComputeEmbedding([FromBody] string text)
        {
            var org = _configuration.GetValue<string>("OPENAI_ORGANIZATION_ID");
            var key = _configuration.GetValue<string>("OPENAI_API_KEY");

            var client = new OpenAI( openAiKey: key , openAiOrg: org);
            var r = client.Embeddings.Create(text);
            if (r.Success)
            {
                return r.Data[0].Embedding;
            }
            else
                return new List<float>();
        }
    }
}