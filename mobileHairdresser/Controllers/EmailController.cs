using mobileHairdresser.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace mobileHairdresser.Controllers
{
    public class EmailController : BaseController
    {
        // GET: Email
        public ActionResult customerEmail()
        {
            if(Session["user"] != null)
            {
                List<tblEmail> unreadtblEmail = db.tblEmails.Where(emails => emails.Read == "false").ToList();


                if (unreadtblEmail.Count != 0)
                {
                    TempData["highlightEmail"] = "true";
                }
                return View(db.tblEmails.OrderByDescending(o=>o.EmailID));
            }
            return RedirectToAction("Index","Home");
        }
        public async Task<ActionResult> ViewEmail(int? EmailID)
        {
            if(Session["user"] != null)
            {
                if(EmailID == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tblEmail tblEmail = await db.tblEmails.FindAsync(EmailID);

                if (tblEmail.Read == "false")
                {
                    tblEmail.Read = "true";

                    db.Entry(tblEmail).State = EntityState.Modified;
                    await db.SaveChangesAsync();  
                }
                return View(tblEmail);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}