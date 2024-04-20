using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace fAI.WebApi.Controllers
{
    // https://portal.azure.com/#@fredericaltorreslive.onmicrosoft.com/resource/subscriptions/57646804-986c-47e8-af66-a3abec32e52a/resourceGroups/ftorres/providers/Microsoft.Web/sites/fAIWebApi/appServices
    // KUDU https://faiwebapi.scm.azurewebsites.net/
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

        public EmbeddingController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }


        [HttpGet(Name = "GetEmbedding")]
        public IEnumerable<float> GetEmbedding()
        {
            return  new List<float>() { 1f,2f,3f,4f,5f}; 
        }

        [HttpPost(Name = "ComputeEmbedding")]
        public IEnumerable<float> ComputeEmbedding([FromBody] string text)
        {
            var client = new OpenAI();
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