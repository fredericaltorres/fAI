using DynamicSugar;
using fAI.Pinecone.Model;
using System.Data.SqlClient;
using static fAI.LeonardoImage;

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
        public string __primaryKey { get; set; }

        public virtual string GetTableName()
        {
            return this.GetType().Name; 
        }

        public virtual List<string> GetColumns()
        {
            return DS.Dictionary(this).Keys.ToList();
        }

        public virtual string GetSqlSelect()
        {
            var cols = this.GetColumns();
            return $"select {string.Join(", ", cols)} from {this.GetTableName()}";
        }

        public virtual string GetSqlSelectById(int id)
        {
            var cols = this.GetColumns().Where(c => !c.StartsWith("__")).ToList();
            return $"select {string.Join(", ", cols)} from {this.GetTableName()} where {__primaryKey} = {id}";
        }

        private static string SqlValue(object value)
        {
            if (value is string)
                return $"'{value.ToString().Replace("'", "''")}'";
            else if (value is DateTime dt)
                return $"'{dt:yyyy-MM-dd HH:mm:ss}'";
            else if (value is bool b)
                return b ? "1" : "0";
            else if (value is Enum e)
                return $"'{e.ToString()}'";
            else if (value == null)
                return "NULL";
            else
                return value.ToString();
        }

        public virtual string GetSqlInsert()
        {
            var nameValues = DS.Dictionary(this);
            if(!string.IsNullOrEmpty(__primaryKey))
                nameValues.Remove(__primaryKey);

            nameValues = nameValues.Where(kv => !kv.Key.StartsWith("__")).ToDictionary(kv => kv.Key, kv => kv.Value);

            var columns = string.Join(", ", nameValues.Keys);
            var values = string.Join(", ", nameValues.Values.Select(v => SqlValue(v)));
            return $@"
                insert into {this.GetTableName()} ({columns}) values ({values}); 
                select SCOPE_IDENTITY();
            ";
        }
    }

    public enum AppointmentStatus
    {
        proposed, pending, booked, arrived, fulfilled, cancelled, noshow
    }

    public class Appointment : BookerEntity
    {
        public int AppointmentId { get; set; }
        public int SlotId { get; set; }
        public int PatientId { get; set; }
        public AppointmentStatus Status { get; set; }
        public string AppointmentType { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public Appointment()
        {
            __primaryKey = "AppointmentId";
        }

        public Appointment Load(SqlDataReader reader)
        {
            AppointmentId = (int)reader["AppointmentId"];
            SlotId = (int)reader["SlotId"];
            PatientId = (int)reader["PatientId"];

            var statusStr = reader.GetString(reader.GetOrdinal("status"));
            Status = Enum.Parse<AppointmentStatus>(statusStr);

            Description = null;
            if (!reader.IsDBNull(reader.GetOrdinal("Description")))
                Description = reader.GetString(reader.GetOrdinal("Description"));

            AppointmentType = null;
            if (!reader.IsDBNull(reader.GetOrdinal("AppointmentType")))
                AppointmentType = reader.GetString(reader.GetOrdinal("AppointmentType"));

            CreatedDate = (DateTime)reader["CreatedDate"];
            ModifiedDate = (DateTime)reader["ModifiedDate"];

            return this;
        }
    }

    public enum SlotStatus
    {
        free, busy, busy_unavailable, busy_tentative, not_available
    }

    public class FreeSlot : BookerEntity
    {
        public string DoctorName { get; set; }
        public int SlotId { get; set; }
        public SlotStatus Status { get; set; }
        public string DayOfWeek { get; set; }
        public DateTime StartDateTime { get; set; }
        public int PractitionerId { get; set; }
        public int ScheduleId { get; set; }

        public string _practitionerLastName { get; }

        public FreeSlot Load(SqlDataReader reader)
        {
            DoctorName = reader.GetString(reader.GetOrdinal("DoctorName"));
            SlotId = (int)reader["SlotId"];

            var statusStr = reader.GetString(reader.GetOrdinal("status"));
            Status = Enum.Parse<SlotStatus>(statusStr);

            DayOfWeek = reader.GetString(reader.GetOrdinal("DayOfWeek"));
            StartDateTime = (DateTime)reader["startDateTime"];
            PractitionerId = (int)reader["PractitionerId"];
            ScheduleId = (int)reader["ScheduleId"];

            return this;
        }

        public FreeSlot(string practitionerLastName)
        {
            _practitionerLastName = practitionerLastName;
        }

        public override string GetSqlSelect()
        {
            return @$"
select 
	p.FirstName + ' ' + p.LastName AS DoctorName,
	sl.SlotId, sl.status,
	DATENAME(WEEKDAY, sl.StartDateTime) AS DayOfWeek,
	sl.startDateTime,
	p.PractitionerId,
	sch.ScheduleId
from Schedule sch
join Practitioner p on p.PractitionerId = sch.PractitionerId
join dbo.Slot sl ON sch.ScheduleId = sl.ScheduleId  and sl.isEnabled = 1
where 
	sl.startDateTime > getdate() and
	sl.status = 'free' and
	p.LastName like '%{_practitionerLastName}%'
";
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
        public static List<FreeSlot> GetFreeSlots(string practitionerLastName)
        {
            var r = new List<FreeSlot>();
            var sql = new FreeSlot(practitionerLastName).GetSqlSelect();
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        r.Add(new FreeSlot(practitionerLastName).Load(reader));
                }
            }
            return r;
        }

        public static List<Patient> GetPatients()
        {
            var r = new List<Patient>();
            var sql = new Patient().GetSqlSelect();
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        r.Add(new Patient().Load(reader));
                }
            }
            return r;
        }

        public static List<Practitioner> GetPractitioners()
        {
            var r = new List<Practitioner>();
            var sql = new Practitioner().GetSqlSelect();
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        r.Add(new Practitioner().Load(reader));
                }
            }
            return r;
        }

        public static Appointment GetAppointmentById(int appointmentId)
        {
            var r = new List<Appointment>();
            var sql = new Appointment().GetSqlSelectById(appointmentId);
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        r.Add(new Appointment().Load(reader));
                }
            }
            return r[0];
        }

        public static List<Appointment> GetAppointments()
        {
            var r = new List<Appointment>();
            var sql = new Appointment().GetSqlSelect();
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        r.Add(new Appointment().Load(reader));
                }
            }
            return r;
        }

        internal static Appointment BookAppointment(int slotId, int PatientId)
        {
            var a = new Appointment();
            a.SlotId = slotId;
            a.PatientId = PatientId;
            a.Status = AppointmentStatus.booked;
            a.CreatedDate = DateTime.Now;
            a.ModifiedDate = DateTime.Now;

            var sql = a.GetSqlInsert();

            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
                a.AppointmentId = Convert.ToInt32(command.ExecuteScalar());
                return a;
            }
        }

        internal static bool BookSlot(int slotId, int patientId, SlotStatus status)
        {
            var sql = $"update slot set Status = '{status}', ModifiedDate = getDate() where slotId={slotId}";
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
                var recordUpdatedCount = command.ExecuteNonQuery();
                return recordUpdatedCount > 0;
            }
        }

        private static string GetConnectionString()
        {
            return "Server=dr-booker.database.windows.net;Database=booker;User Id=fredericaltorres;Password=machi123!;";
        }
    }
}