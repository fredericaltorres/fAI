using BookerDatabase;
using Microsoft.AspNetCore.Mvc;

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
            return BookerDB.GetPractitioners();
        }

        // curl.exe -X GET -H "accept: application/json" "https://localhost:7009/BookerDB/Patients"
        [HttpGet("Patients")]
        public IEnumerable<Patient> GetPatients()
        {
            return BookerDB.GetPatients();
        }

        // curl.exe -X GET -H "accept: application/json" "https://localhost:7009/BookerDB/FreeSlots?practitionerLastName=Moreau"
        [HttpGet("FreeSlots")]
        public IEnumerable<FreeSlot> GetFreeSlots([FromQuery]string practitionerLastName)
        {
            return BookerDB.GetFreeSlots(practitionerLastName, DateTime.Now);
        }

        // curl.exe -X PUT -H "accept: application/json" "https://localhost:7009/BookerDB/BookAppointment?slotId=2&patientId=1"
        [HttpPut("BookAppointment")]
        public Appointment BookAppointment([FromQuery] int slotId, [FromQuery] int patientId)
        {
            var a = BookerDB.BookAppointment(slotId, patientId);
            if (a.AppointmentId > 0)
            {
                if(BookerDB.BookSlot(slotId, patientId, SlotStatus.busy))
                {
                    return BookerDB.GetAppointmentById(a.AppointmentId);
                }
            }
            return null;
        }
    }
}