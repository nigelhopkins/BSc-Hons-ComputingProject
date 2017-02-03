using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using mobileHairdresser.Database;

namespace mobileHairdresser.Controllers
{
    public class HomeController : Controller
    {
        public mobileHairdresserEntities db = new mobileHairdresserEntities();

        [HttpPost]
        public ActionResult systemMessage(string systemMessage)
        {
            if(!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("systemCookie"))
            {
                HttpCookie systemCookie = new HttpCookie("systemCookie");
                systemCookie.Value = "Acceptance of cookies was done on :" + DateTime.Now.ToShortDateString()  + " @ " + DateTime.Now.ToShortTimeString();

                this.ControllerContext.HttpContext.Response.Cookies.Add(systemCookie);
            }

            TempData["systemMessage"] = null;

            return RedirectToAction("Index", "Home");
        }
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            if(this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains("systemCookie"))
            {
                return View();
            }
            else
            {
                TempData["systemMessage"] = "<h1>Our Cookie Policy </h1>"
                                            + "We use cookies to give you the best possible experiance on our website.<br />"
                                            + "Read our <a href='"+ @Url.Action("cookiePolicy", "Home")+"'>cookie policy</a> to learn more <br />"
                                            + "<span><button type='submit' class='cookAccept'>Accept</button> | <a href='"+ Request.UrlReferrer + "'>Decline</a></span>";
            }
            return View();
        }
        public ActionResult Prices()
        {
            ViewBag.HaircutTypeName = new SelectList(db.tblHaircutTypes, "TypeID", "TypeName");
            ViewBag.Title = "Prices";
            return View();
        }
        public JsonResult getHaircutTypeID(int TypeID)
        {
            db.Configuration.ProxyCreationEnabled = false;
            return Json(db.tblHaircuts.Where(p => p.TypeID == TypeID), JsonRequestBehavior.AllowGet);
        }
        public ActionResult Portfolio()
        {
            ViewBag.Title = "Portfolio";

            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Title = "Contact";

            return View();
        }
        public ActionResult cookiePolicy()
        {
            ViewBag.Title = "Our Cookie Policy";
            return View();
        }
    }
}
