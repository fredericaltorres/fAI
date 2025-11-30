using BookerDB;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Formatters;
//using Microsoft.Extensions.Caching.Memory;
//using static System.Net.Mime.MediaTypeNames;

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

        // curl.exe -X GET -H "accept: application/json" "https://localhost:7009/BookerDB/Practitioners"
        [HttpGet("Practitioners")]
        public IEnumerable<Practitioner> GetPractitioners()
        {
            return BookerDB2.GetPractitioners();
        }

        // curl.exe -X GET -H "accept: application/json" "https://localhost:7009/BookerDB/Patients"
        [HttpGet("Patients")]
        public IEnumerable<Patient> GetPatients()
        {
            return BookerDB2.GetPatients();
        }

        // curl.exe -X GET -H "accept: application/json" "https://localhost:7009/BookerDB/FreeSlots?practitionerLastName=Moreau"
        [HttpGet("FreeSlots")]
        public IEnumerable<FreeSlot> GetFreeSlots([FromQuery]string practitionerLastName)
        {
            return BookerDB2.GetFreeSlots(practitionerLastName);
        }
    }
}