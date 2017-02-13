using mobileHairdresser.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace mobileHairdresser.Controllers
{
    public class BaseController : Controller
    {
        public mobileHairdresserEntities db = new mobileHairdresserEntities();
        // GET: Baase
        public BaseController()
        {
            List<tblEmail> getUnreadEmails = db.tblEmails.Where(email => email.Read == "false").ToList();
            if (getUnreadEmails.Count != 0)
            {
                ViewBag.emailCount = "glyphicon glyphicon-envelope";
            }
            
        }
    }
}