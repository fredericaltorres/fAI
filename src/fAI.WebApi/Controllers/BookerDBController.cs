using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Caching.Memory;
using System.Data.SqlClient;
using static System.Net.Mime.MediaTypeNames;

namespace fAI.WebApi.Controllers
{
    // https://portal.azure.com/#@fredericaltorreslive.onmicrosoft.com/resource/subscriptions/57646804-986c-47e8-af66-a3abec32e52a/resourceGroups/ftorres/providers/Microsoft.Web/sites/fAIWebApi/configuration
    // KUDU https://faiwebapi.scm.azurewebsites.net/Env.cshtml
    // https://faiwebapi.azurewebsites.net/Embedding


    //  curl.exe -X GET  -H "accept: text/plain" "https://localhost:7009/InMemoryLog/rows"
    //  curl.exe -X PUT  -H "accept: text/plain" "https://localhost:7009/InMemoryLog/clear"
    //  curl.exe -X POST -H "accept: * / *" -H "Content-Type: application/json"  -d "\"tutu\""  "https://localhost:7009/InMemoryLog/rows"


    //  curl.exe -X GET   "https://faiwebapi.azurewebsites.net/InMemoryLog/rows"
    //  curl.exe -X PUT   "https://faiwebapi.azurewebsites.net/InMemoryLog/clear"
    //  curl.exe -X POST -H "Content-Type: application/json"   -d "\"Zizi\""  "https://faiwebapi.azurewebsites.net/InMemoryLog/rows"
    //  curl.exe -X POST -H "accept: * / *" -H "Content-Type: application/json"  -d "\"Zizi\""  "https://faiwebapi.azurewebsites.net/InMemoryLog/rows"

    // curl.exe -X GET -H "accept: application/json" "https://localhost:7009/BookerDB"

    public class Practitioner
    {
        public int PractitionerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialty { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public Practitioner Load(SqlDataReader reader)
        {
            PractitionerId = (int)reader["PractitionerId"];
            FirstName = reader["FirstName"].ToString();
            LastName = reader["LastName"].ToString();
            Specialty = reader["Specialty"].ToString();
            IsActive = (bool)reader["IsActive"];
            CreatedDate = (DateTime)reader["CreatedDate"];
            ModifiedDate = (DateTime)reader["ModifiedDate"];
            return this;
        }
    };

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
            return __GetPractitioners();
        }

        private List<Practitioner> __GetPractitioners()
        {
            var r = new List<Practitioner>();
            var connectionString = "Server=dr-booker.database.windows.net;Database=booker;User Id=fredericaltorres;Password=machi123!;";
            using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
            {
                connection.Open();
                // Perform database operations here
                var command = new System.Data.SqlClient.SqlCommand("select PractitionerId,FirstName,LastName,Specialty,IsActive,CreatedDate,ModifiedDate from Practitioner", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        r.Add(new Practitioner().Load(reader));
                    }
                }
            }
            return r;
        }
    }
}