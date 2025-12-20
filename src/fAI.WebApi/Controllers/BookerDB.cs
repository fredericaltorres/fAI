using DynamicSugar;
using fAI.Pinecone.Model;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Text.Json.Serialization;
using static fAI.LeonardoImage;

namespace BookerDatabase
{
    public class BookerEntity
    {
        internal string __primaryKey { get; set; }

        internal List<string> __joinedFields = new List<string>();

        public T __GetEnum<T>(SqlDataReader reader, string name) where T : struct, Enum
        {
            var statusStr = reader.GetString(reader.GetOrdinal(name));
            if (Enum.TryParse<T>(statusStr, true, out T result))
            {
                return result;
            }
            throw new ArgumentException($"Unable to parse '{statusStr}' into {typeof(T).Name}.");
        }

        public string __GetString(SqlDataReader reader, string name)
        {
            var s = null as string;
            if (!reader.IsDBNull(reader.GetOrdinal(name)))
                s = reader.GetString(reader.GetOrdinal(name));
            return s;
        }

        public virtual string __GetTableName()
        {
            return this.GetType().Name; 
        }

        public virtual List<string> __GetColumns()
        {
            return DS.Dictionary(this).Keys.ToList();
        }

        public virtual string __GetSqlSelect()
        {
            var cols = this.__GetColumns();
            return $"select {string.Join(", ", cols)} from {this.__GetTableName()}";
        }

        public virtual string __GetSqlSelectById(int id)
        {
            var cols = this.__GetColumns().Where(c => !c.StartsWith("__")).ToList();
            return $"select {string.Join(", ", cols)} from {this.__GetTableName()} where {__primaryKey} = {id}";
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

        public virtual string __GetSqlInsert()
        {
            var nameValues = DS.Dictionary(this);
            if(!string.IsNullOrEmpty(__primaryKey))
                nameValues.Remove(__primaryKey);

            foreach(var joinField in __joinedFields)
                nameValues.Remove(joinField);

            nameValues = nameValues.Where(kv => !kv.Key.StartsWith("__")).ToDictionary(kv => kv.Key, kv => kv.Value);

            var columns = string.Join(", ", nameValues.Keys);
            var values = string.Join(", ", nameValues.Values.Select(v => SqlValue(v)));
            return $@"
                insert into {this.__GetTableName()} ({columns}) values ({values}); 
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

        // Joined fields
        public int PractitionerId { get; set; }
        public string ScheduleName { get; set; }
        public string  PractitionerFirstLastName { get; set; }
        public DateTime StartDateTime { get; set; }
        public SlotStatus SlotStatus { get; set; }

        public Appointment()
        {
            __primaryKey = "AppointmentId";
            __joinedFields = new List<string> { "PractitionerId", "ScheduleName", "PractitionerFirstLastName", "StartDateTime", "SlotStatus" };
        }

        public override string __GetSqlSelectById(int id)
        {
            var cols = this.__GetColumns().Where(c => !c.StartsWith("__")).ToList();
            return $@"
select a.*,
    sch.practitionerId, 
    sch.ScheduleName, 
    pr.FirstName + ' ' + pr.LastName practitionerFirstLastName,
    sl.StartDateTime, 
    sl.Status SlotStatus
from Appointment a
join slot sl on a.slotId = sl.SlotId and sl.IsEnabled = 1
join Schedule sch on sl.ScheduleId = sch.ScheduleId and sch.IsActive = 1
join Patient p on a.PatientId = p.PatientId
join practitioner pr on sch.practitionerId = pr.practitionerId and pr.IsActive = 1
where {__primaryKey} = {id}
";
        }

        public Appointment Load(SqlDataReader reader)
        {
            AppointmentId = (int)reader["AppointmentId"];
            SlotId = (int)reader["SlotId"];
            PatientId = (int)reader["PatientId"];
            Status = __GetEnum<AppointmentStatus>(reader, "status");
            Description = __GetString(reader, "Description");
            AppointmentType = __GetString(reader, "AppointmentType");
            CreatedDate = (DateTime)reader["CreatedDate"];
            ModifiedDate = (DateTime)reader["ModifiedDate"];

            // Join fields
            PractitionerId = (int)reader["PractitionerId"];
            ScheduleName = __GetString(reader, "ScheduleName");
            AppointmentType = __GetString(reader, "AppointmentType");
            StartDateTime = (DateTime)reader["StartDateTime"];
            SlotStatus = __GetEnum<SlotStatus>(reader, "SlotStatus");

            return this;
        }
    }

    public enum SlotStatus
    {
        free, busy, busy_unavailable, busy_tentative, not_available
    }

    public class FreeSlot : BookerEntity
    {
        public string PractitionerName { get; set; }
        public int SlotId { get; set; }
        public SlotStatus Status { get; set; }
        public string DayOfWeek { get; set; }
        public DateTime StartDateTime { get; set; }
        public int PractitionerId { get; set; }
        public int ScheduleId { get; set; }

        public string _practitionerLastName { get; }

        public FreeSlot Load(SqlDataReader reader)
        {
            PractitionerName = __GetString(reader, "PractitionerName");
            SlotId = (int)reader["SlotId"];
            Status = __GetEnum<SlotStatus>(reader, "Status");
            DayOfWeek = __GetString(reader, "DayOfWeek");
            StartDateTime = (DateTime)reader["startDateTime"];
            PractitionerId = (int)reader["PractitionerId"];
            ScheduleId = (int)reader["ScheduleId"];

            return this;
        }

        public FreeSlot(string practitionerLastName)
        {
            _practitionerLastName = practitionerLastName;
        }

        public override string __GetSqlSelect()
        {
            return @$"
                select 
	                p.FirstName + ' ' + p.LastName AS PractitionerName,
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

    public class BookerDB
    {
        public static bool AreDateAround(DateTime refDate, DateTime testDate, int acceptedRangeInDays = 2)
        {
            TimeSpan difference = testDate - refDate; // Calculate the difference in days between the two dates
            return Math.Abs(difference.TotalDays) <= acceptedRangeInDays; // Check if the absolute difference is within 2 days
        }

        public static List<FreeSlot> GetFreeSlots(string practitionerLastName, DateTime aroundDate)
        {
            var r = new List<FreeSlot>();
            var sql = new FreeSlot(practitionerLastName).__GetSqlSelect();
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        r.Add(new FreeSlot(practitionerLastName).Load(reader));
                }
            }

            r = r.Where(fs => AreDateAround(aroundDate, fs.StartDateTime)).ToList();
            return r;
        }

        public static List<Patient> GetPatients()
        {
            var r = new List<Patient>();
            var sql = new Patient().__GetSqlSelect();
            using (var connection = GetConnection())
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
            var sql = new Practitioner().__GetSqlSelect();
            using (var connection = GetConnection())
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
            var sql = new Appointment().__GetSqlSelectById(appointmentId);
            using (var connection = GetConnection())
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
            var sql = new Appointment().__GetSqlSelect();
            using (var connection = GetConnection())
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

        internal static Appointment BookAppointment(int slotId, int patientId)
        {
            var a = new Appointment()
            {
                SlotId = slotId,
                PatientId = patientId,
                Status = AppointmentStatus.booked,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                var command = new SqlCommand(a.__GetSqlInsert(), connection);
                try
                {
                    a.AppointmentId = Convert.ToInt32(command.ExecuteScalar());
                }
                catch (SqlException sex)
                {
                    var duplicateAppointment = sex.Message.Contains("duplicate key");
                    if (duplicateAppointment)
                        throw new ApplicationException($"Duplicate appointment for SlotId={slotId} and PatientId={patientId}", sex);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Error occurred while trying to book the appointment.", ex);
                }

                return a;
            }
        }

        internal static bool BookSlot(int slotId, int patientId, SlotStatus status)
        {
            var sql = $"update slot set Status = '{status}', ModifiedDate = getDate() where slotId={slotId}";
            using (var connection = GetConnection())
            {
                connection.Open();
                var command = new SqlCommand(sql, connection);
                var recordUpdatedCount = command.ExecuteNonQuery();
                return recordUpdatedCount > 0;
            }
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        private static string GetConnectionString()
        {
            return "Server=dr-booker-db.database.windows.net;Database=dr-booker-db;User Id=fredericaltorres;Password=machi123!;";
        }
    }
}