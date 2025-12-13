using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using static System.Net.Mime.MediaTypeNames;

namespace fAI.WebApi.Controllers
{
    // https://portal.azure.com/#@fredericaltorreslive.onmicrosoft.com/resource/subscriptions/57646804-986c-47e8-af66-a3abec32e52a/resourceGroups/ftorres/providers/Microsoft.Web/sites/fAIWebApi/configuration
    // KUDU https://faiwebapi.scm.azurewebsites.net/Env.cshtml
    // https://faiwebapi.azurewebsites.net/Embedding
    // https://faiwebapi2026-ahe0dmhrasdpd5cx.canadacentral-01.azurewebsites.net/Embedding
    // curl.exe -i -X POST -H "Content-Type: application/json"   -d "sea"  "https://faiwebapi2026-ahe0dmhrasdpd5cx.canadacentral-01.azurewebsites.net/Embedding"

    /*
        When deploying 
    1.Environment Variables
        OPENAI_ORGANIZATION_ID, OPENAI_API_KEY
    2. CORS settings to allow your front-end to call the API
        "https://ftorres.azurewebsites.net"
     
     */

    [ApiController]
    [Route("[controller]")]
    public class EmbeddingController : ControllerBase
    {

        private readonly IMemoryCache _memoryCache;

        private IConfiguration _configuration;

        public EmbeddingController(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _configuration = configuration;
            _memoryCache = memoryCache;
            fAI.Logger.DefaultLogFileName = Path.Combine(Path.GetTempPath(), "fAI.log");
        }

        [HttpGet("clear")]
        public void Clear()
        {
            var remoteIpAddress = base.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            _memoryCache.Remove(remoteIpAddress);
        }

        [HttpGet(Name = "GetEmbedding")]
        public IEnumerable<float> GetEmbedding()
        {
            return  new List<float>() { 1f,2f,3f,4f,5f}; 
        }

        [HttpPost(Name = "ComputeEmbedding")]
        public IEnumerable<float> ComputeEmbedding([FromBody] string text)
        {
            if (GetCallCounter() < 100) // avoid crazy calling my api costing me money
            {
                var org = _configuration.GetValue<string>("OPENAI_ORGANIZATION_ID");
                var key = _configuration.GetValue<string>("OPENAI_API_KEY");

                var client = new OpenAI(openAiKey: key, openAiOrg: org);
                var r = client.Embeddings.Create(text);
                if (r.Success)
                {
                    return r.Data[0].Embedding;
                }
                else
                    return new List<float>();
            }
            else throw new Exception("Too many calls");
        }

        private int GetCallCounter()
        {
            var remoteIpAddress = base.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            int callCounter = 0;
            _memoryCache.TryGetValue(remoteIpAddress, out callCounter);
            callCounter += 1;
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(5));
            _memoryCache.Set(remoteIpAddress, callCounter, cacheEntryOptions);

            return callCounter;
        }
    }
}