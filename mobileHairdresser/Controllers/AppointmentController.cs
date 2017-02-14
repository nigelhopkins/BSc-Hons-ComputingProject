using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace mobileHairdresser.Controllers
{
    public class AppointmentController : BaseController
    {
        // GET: Appointment
        public ActionResult appointmentIndex()
        {
            return View();
        }
    }
}