using Microsoft.SqlServer.Server;
using mobileHairdresser.Database;
using mobileHairdresser.Model;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
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
            ViewBag.employeeName = new SelectList(db.tblEmployees, "employeeID", "FirstName");
            //ViewBag.Title = "Make new appointment";
            ViewBag.haircutList = new SelectList(db.tblHaircuts, "HaircutID", "HaircutName");

            return View();
        }
        public JsonResult getTimeSlots(string appointmentDate, tblTimeSlot tblTimeSlot)
        {
            DateTime setDate = Convert.ToDateTime(appointmentDate);

            List<int> tempTimeSlot = db.tblAppointments.Where(x => x.appointmentDate == setDate).Select(q => q.timeSlotID).ToList();

            var getTimeSlotIDs = db.tblTimeSlots.Where(q => !tempTimeSlot.Contains(q.timeSlotID));

            return Json(new SelectList(getTimeSlotIDs, "timeSlotID", "timeSlot"), JsonRequestBehavior.AllowGet);


        }
        [HttpPost]
        public ActionResult bookAppointment(tblAppointment tblAppointment, tblClient tblClient)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    int appointmentTime = int.Parse(Request.Form["timeSlotID"]);
                    tblClient newClient = new tblClient();
                    {
                        newClient.clientName = tblAppointment.tblClient.clientName;
                        newClient.clientMobile = tblAppointment.tblClient.clientMobile;
                        newClient.clientEmail = tblAppointment.tblClient.clientEmail;
                        newClient.clientHouseNumber = tblAppointment.tblClient.clientHouseNumber;
                        newClient.clientPostalCode = tblAppointment.tblClient.clientPostalCode;
                    }

                    tblAppointment newAppointment = new tblAppointment();
                    {
                        newAppointment.employeeID = tblAppointment.employeeID;
                        newAppointment.haircutID = tblAppointment.haircutID;
                        newAppointment.timeSlotID = tblAppointment.timeSlotID;
                    }
                    db.tblAppointments.Add(tblAppointment);
                    db.SaveChanges();
                    return RedirectToAction("confirmationEmail", "Appointment", new { appointmentID = tblAppointment.appointmentID });
                }
                catch (Exception)
                {
                    TempData["systemMessage"] = "<p>Unable to make apppointment please try again later.</p>";
                }
            }

            return RedirectToAction("appointmentIndex", "Appointment");
        }
        public async Task<ActionResult> appointmentConfirmation(int? appointmentID)
        {
            if (appointmentID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblAppointment tblAppointment = await db.tblAppointments.FindAsync(appointmentID);
            tblEmployee tblEmployee = await db.tblEmployees.FindAsync(tblAppointment.employeeID);
            tblHaircut tblHaircut = await db.tblHaircuts.FindAsync(tblAppointment.haircutID);
            tblTimeSlot tblTimeSlot = await db.tblTimeSlots.FindAsync(tblAppointment.timeSlotID);
            ViewData["employee"] = tblEmployee.FirstName + " " + tblEmployee.LastName;
            ViewData["haircut"] = tblHaircut.HaircutName;
            ViewData["timeSlot"] = tblTimeSlot.timeSlot;

            if (tblAppointment == null)
            {
                return HttpNotFound();
            }
            return View(tblAppointment);
        }
        public async Task<ActionResult> printAppointmentConfirmation(int? appointmentID)
        {
            if (appointmentID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tblAppointment tblAppointment = await db.tblAppointments.FindAsync(appointmentID);
            tblEmployee tblEmployee = await db.tblEmployees.FindAsync(tblAppointment.employeeID);
            tblHaircut tblHaircut = await db.tblHaircuts.FindAsync(tblAppointment.haircutID);
            tblTimeSlot tblTimeSlot = await db.tblTimeSlots.FindAsync(tblAppointment.timeSlotID);
            ViewData["employee"] = tblEmployee.FirstName + " " + tblEmployee.LastName;
            ViewData["haircut"] = tblHaircut.HaircutName;
            ViewData["timeSlot"] = tblTimeSlot.timeSlot;

            if (tblAppointment == null)
            {
                return HttpNotFound();
            }
            return View(tblAppointment);
        }
        public ActionResult createAppointmentCard(int? appointmentID)
        {
            return new UrlAsPdf(Url.Action("printAppointmentConfirmation", "Appointment") + "/?appointmentID=" + appointmentID)
            {
                FileName = "Mobile Hairdresser Appointment Card.pdf",
                PageOrientation = Rotativa.Options.Orientation.Landscape,
                PageSize = Rotativa.Options.Size.A5,
                PageMargins = new Rotativa.Options.Margins(5, 5, 5, 5)

            };
        }
        public ActionResult confirmationEmail(int appointmentID)
        {
            string body;

            tblAppointment tblAppointment = db.tblAppointments.Find(appointmentID);
            tblHaircut tblHaircut = db.tblHaircuts.Find(tblAppointment.haircutID);
            tblEmployee tblEmployee = db.tblEmployees.Find(tblAppointment.employeeID);
            tblTimeSlot tblTimeSlot = db.tblTimeSlots.Find(tblAppointment.timeSlotID);

            using (var stream = new StreamReader(Server.MapPath("//App_Data//emailTemplates/" + "appointmentConfirmation.html")))
            {
                body = stream.ReadToEnd();
            }

            try
            {
                string appointmentDate = tblAppointment.appointmentDate.DayOfWeek.ToString() + " " + tblAppointment.appointmentDate.Day + " " + tblAppointment.appointmentDate.ToString("MMMM") + " " + tblAppointment.appointmentDate.Year;
                TimeSpan appointmentTime = tblTimeSlot.timeSlot;
                string haircutName = tblHaircut.HaircutName;
                string employeeName = tblEmployee.FirstName + " " + tblEmployee.LastName;
                string customerName = tblAppointment.tblClient.clientName;
                string customerPhoneNumber = tblAppointment.tblClient.clientMobile;
                string customerEmail = tblAppointment.tblClient.clientEmail;
                int customerHouseNumber = Convert.ToInt32(tblAppointment.tblClient.clientHouseNumber);
                string customerAddress = tblAppointment.tblClient.clientPostalCode;
                string emailFrom = ConfigurationManager.AppSettings["EmailFormAddress"];

                MailMessage confirmationMessage = new MailMessage();
                confirmationMessage.To.Add(customerEmail);
                confirmationMessage.From = new MailAddress(emailFrom, "Mobile Hairdresser");
                confirmationMessage.ReplyToList.Add(new MailAddress(customerEmail));
                confirmationMessage.Subject = @"Mobile Hairdresser : Appointment Confirmation";
                confirmationMessage.Body = string.Format(body, customerName, customerPhoneNumber, customerEmail
                                                        , customerHouseNumber, customerAddress, appointmentID, appointmentDate
                                                        , appointmentTime, employeeName, haircutName);
                confirmationMessage.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient())
                {
                    smtp.EnableSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableSSL"]);
                    smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                    smtp.Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings["MailAuthUser"], ConfigurationManager.AppSettings["MailAuthPass"]);
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.SendCompleted += (s, e) => { smtp.Dispose(); };
                    smtp.Send(confirmationMessage);
                }
            }
            catch (Exception emailError)
            {
                TempData["appointmentConfirmationEmail"] = "An error has occured and your email could not be sent!" + emailError;
                return RedirectToAction("appointmentConfirmation", "Appointment", new { appointmentID = tblAppointment.appointmentID });
            }

            TempData["appointmentConfirmationEmail"] = "We have sent you an email confirming your appointment.";
            return RedirectToAction("appointmentConfirmation", "Appointment", new { appointmentID = tblAppointment.appointmentID });

        }

        public ActionResult Diary(string appointmentDate)
        {
            if (Session["user"] != null)
                {
                    DateTime date = Convert.ToDateTime(appointmentDate);
                    if (appointmentDate != null)
                    {
                        int numOfDays = Convert.ToInt32((date - DateTime.Today).TotalDays);

                        if (numOfDays > 42)
                        {
                            date = date.AddDays(-1);
                        }
                        else if (numOfDays < 0)
                        {
                            date = date.AddDays(+1);
                        }
                        else if (numOfDays <= 42 && numOfDays >= 0)
                        {
                            bool findDate = db.tblAppointments.Where(x => x.appointmentDate.Equals(date)).Any();

                            ViewBag.appointmentDate = date.ToString("dddd dd MM yyyy");

                            if (findDate != false)
                            {
                                return View(db.tblAppointments.Where(x => x.appointmentDate.Equals(date)).ToList());
                            }
                            else
                            {
                            TempData["AppointmentError"] = "No appointments have been booked for this day!";
                                return View(db.tblAppointments.ToList());
                            }
                        }
                        else
                        {
                            bool findDate = db.tblAppointments.Where(x => x.appointmentDate.Equals(date)).Any();

                            ViewBag.appointmentDate = date.ToString("dddd dd MM yyyy");

                            if (findDate != false)
                            {
                                return View(db.tblAppointments.Where(x => x.appointmentDate.Equals(date)).ToList());
                            }
                            else
                            {
                            TempData["AppointmentError"] = "No appointments have been book for this day!";
                            return View(db.tblAppointments.ToList());
                            }
                    }
                    }
                    else
                    {
                        return View(db.tblAppointments.ToList());
                    }
                }
                return RedirectToAction("Index", "Home");
        }
        public ActionResult Search(int? appointmentID)
        {
            if (appointmentID != null)
            {
                bool findAppointment = db.tblAppointments.Where(a => a.appointmentID == appointmentID).Any();
                if (findAppointment != false)
                {
                    tblAppointment tblAppointment = db.tblAppointments.Find(appointmentID);
                    tblEmployee tblEmployee = db.tblEmployees.Find(tblAppointment.employeeID);
                    tblHaircut tblHaircut = db.tblHaircuts.Find(tblAppointment.haircutID);
                    tblTimeSlot tblTimeSlot = db.tblTimeSlots.Find(tblAppointment.timeSlotID);
                    ViewData["employee"] = tblEmployee.FirstName + " " + tblEmployee.LastName;
                    ViewData["haircut"] = tblHaircut.HaircutName;
                    ViewData["timeSlot"] = tblTimeSlot.timeSlot;

                    return View(tblAppointment);
                }
                else
                {
                    ViewData["appointmentError"] = "No appointment found please try again";
                }

            }
            return View();
        }
        public ActionResult changeAppointment(int? appointmentID)
        {
            ViewBag.employeeName = new SelectList(db.tblEmployees, "employeeID", "FirstName");
            //ViewBag.Title = "Make new appointment";
            ViewBag.haircutList = new SelectList(db.tblHaircuts, "HaircutID", "HaircutName");
            tblAppointment tblAppointment = db.tblAppointments.Find(appointmentID);
            return View(tblAppointment);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult saveChangeAppointment(int? appointmentID, tblAppointment tblAppointment)
        {
            try
            {
                var findClientID = db.tblAppointments.Where(clientID => clientID.appointmentID == appointmentID).First();
                tblClient newClient = db.tblClients.Find(findClientID.clientID);
                {
                    newClient.clientName = tblAppointment.tblClient.clientName;
                    newClient.clientMobile = tblAppointment.tblClient.clientMobile;
                    newClient.clientEmail = tblAppointment.tblClient.clientEmail;
                    newClient.clientHouseNumber = tblAppointment.tblClient.clientHouseNumber;
                    newClient.clientPostalCode = tblAppointment.tblClient.clientPostalCode;
                }

                tblAppointment newAppointment = db.tblAppointments.Find(tblAppointment.appointmentID);
                {
                    newAppointment.employeeID = tblAppointment.employeeID;
                    newAppointment.haircutID = tblAppointment.haircutID;
                    newAppointment.timeSlotID = tblAppointment.timeSlotID;
                }

                db.SaveChanges();
                return RedirectToAction("confirmationEmail", "Appointment", new { appointmentID = tblAppointment.appointmentID });
            }
            catch (Exception)
            {
                TempData["systemMessage"] = "<p>Unable to make apppointment please try again later.</p>";
            }
            return RedirectToAction("Search", "Appointment");
        }
        [HttpGet]
        public async Task<ActionResult> cancelAppointment(int? appointmentID)
        {
            tblAppointment tblAppointment = await db.tblAppointments.FindAsync(appointmentID);
            db.tblClients.Remove(tblAppointment.tblClient);
            db.tblAppointments.Remove(tblAppointment);
            await db.SaveChangesAsync();

            return RedirectToAction("Search", "Appointment");
        }
        [HttpPost]
        public async Task<ActionResult> cancelAppointmentConfirmation(int? appointmentID)
        {
            tblAppointment tblAppointment = await db.tblAppointments.FindAsync(appointmentID);
            db.tblAppointments.Remove(tblAppointment);
            await db.SaveChangesAsync();
            return RedirectToAction("Search", "Appointment");
        }
        public ActionResult getDirections(int? appointmentID)
        {
            if (Session["user"] != null)
            {
                return View(db.tblAppointments.Find(appointmentID));
            }

            return RedirectToAction("Search", "Appointment", new { appointmentID = appointmentID });     

        }
        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                db.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}