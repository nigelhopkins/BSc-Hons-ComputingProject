using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace mobileHairdresser.Model
{
    public class getDiaryDetails
    {
        public DateTime appointmentDate { get; set; }
        public IEnumerable<Database.tblAppointment> tblAppointments { get; set; }
    }
}