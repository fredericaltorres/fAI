using BookerDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Caching.Memory;
using static System.Net.Mime.MediaTypeNames;

namespace fAI.WebApi.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class BookerDBController : ControllerBase
    {
        private IConfiguration _configuration;

        public BookerDBController(IConfiguration configuration)
        {
            _configuration = configuration;
            fAI.Logger.DefaultLogFileName = Path.Combine(Path.GetTempPath(), "fAI.log");
        }

        [HttpGet(Name = "GetPractitioners")]
        public IEnumerable<Practitioner> GetPractitioners()
        {
            var c = Practitioner.GetColumns();
            return BookerDB2.GetPractitioners();
        }
    }
}