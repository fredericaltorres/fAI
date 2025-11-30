using DynamicSugar;
using System.Data.SqlClient;

namespace BookerDB
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

    

    public class BookerEntity
    {
        public string GetTableName()
        {
            return this.GetType().Name; 
        }

        public List<string> GetColumns()
        {
            return DS.Dictionary(this).Keys.ToList();
        }

        public string GetSqlSelect()
        {
            var cols = this.GetColumns();
            return $"select {string.Join(", ", cols)} from {this.GetTableName()}";
        }
    }

    public class Patient : BookerEntity
    {
        public int PatientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public Patient Load(SqlDataReader reader)
        {
            PatientId = (int)reader["PatientId"];
            FirstName = reader.GetString(reader.GetOrdinal("FirstName"));
            LastName = reader.GetString(reader.GetOrdinal("LastName"));
            DateOfBirth = (DateTime)reader["DateOfBirth"];
            Phone = reader.GetString(reader.GetOrdinal("Phone"));
            Email = reader.GetString(reader.GetOrdinal("Email"));
            IsActive = (bool)reader["IsActive"];
            CreatedDate = (DateTime)reader["CreatedDate"];
            ModifiedDate = (DateTime)reader["ModifiedDate"];
            return this;
        }
    }

    public class Practitioner: BookerEntity
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
            FirstName = reader.GetString(reader.GetOrdinal("FirstName"));
            LastName = reader.GetString(reader.GetOrdinal("LastName"));
            Specialty = reader.GetString(reader.GetOrdinal("Specialty"));
            IsActive = (bool)reader["IsActive"];
            CreatedDate = (DateTime)reader["CreatedDate"];
            ModifiedDate = (DateTime)reader["ModifiedDate"];
            return this;
        }
    };

    public class BookerDB2
    {

        public static List<Patient> GetPatients()
        {
            var r = new List<Patient>();
            var sql = new Patient().GetSqlSelect();
            string connectionString = GetConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        r.Add(new Patient().Load(reader));
                    }
                }
            }
            return r;
        }

        public static List<Practitioner> GetPractitioners()
        {
            var r = new List<Practitioner>();
            var sql = new Practitioner().GetSqlSelect();
            string connectionString = GetConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
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

        private static string GetConnectionString()
        {
            return "Server=dr-booker.database.windows.net;Database=booker;User Id=fredericaltorres;Password=machi123!;";
        }
    }
}