using mobileHairdresser.Database;
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
        public ActionResult createNewAppointment()
        {
            List<tblHaircut> haircutList = db.tblHaircuts.ToList();
            ViewBag.haircutList = new SelectList(haircutList, "HaircutID", "HaircutName");
            return View();
        }
        public ActionResult Diary ()
        {
            return View();
        }
        public ActionResult Search()
        {
            return View();
        }
    }
}