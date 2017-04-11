﻿using mobileHairdresser.Database;
using Rotativa;
using Rotativa.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;

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
        [HttpGet]
        public JsonResult getTimeSlots(string appointmentDate, tblTimeSlot tblTimeSlot)
        {
            DateTime setDate = Convert.ToDateTime(appointmentDate);
            if((setDate - DateTime.Today).TotalDays > 0)
            {
                List<int> tempTimeSlot = db.tblAppointments.Where(x => x.appointmentDate == setDate).Select(q => q.timeSlotID).ToList();

                var getTimeSlotIDs = db.tblTimeSlots.Where(q => !tempTimeSlot.Contains(q.timeSlotID));

                return Json(new SelectList(getTimeSlotIDs, "timeSlotID", "timeSlot"), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult bookAppointment(tblAppointment tblAppointment, tblClient tblClient)
        {

            if (ModelState.IsValid)
            {
                if ((tblAppointment.appointmentDate - DateTime.Today).TotalDays <= 0)
                {
                    ViewData["appointmentError"] = "We are unable to create appointment for the same day.";

                    if (Request.UrlReferrer.ToString() != null)
                    {
                        return Redirect(Request.UrlReferrer.ToString());
                    }
                    else
                    {
                        return RedirectToAction("appointmentIndex", "Appointment");
                    }
                }
                else
                {
                    try
                    {
                        var salt = Crypto.GenerateSalt();
                        var generatedPassword = Membership.GeneratePassword(12, 1);
                        TempData["userInfo"] = generatedPassword.ToString();
                        int appointmentTime = int.Parse(Request.Form["timeSlotID"]);
                        tblClient newClient = new tblClient();
                        {
                            newClient.clientName = tblAppointment.tblClient.clientName;
                            newClient.clientMobile = tblAppointment.tblClient.clientMobile;
                            newClient.clientEmail = tblAppointment.tblClient.clientEmail;
                            newClient.clientHouseNumber = tblAppointment.tblClient.clientHouseNumber;
                            newClient.clientPostalCode = tblAppointment.tblClient.clientPostalCode;
                            newClient.salt = salt;
                            newClient.Password = Crypto.HashPassword(generatedPassword + salt).ToString();
                        }
                        db.tblClients.Add(newClient);
                        db.SaveChanges();

                        tblAppointment newAppointment = new tblAppointment();
                        {
                            newAppointment.appointmentDate = tblAppointment.appointmentDate;
                            newAppointment.clientID = newClient.clientID;
                            newAppointment.employeeID = tblAppointment.employeeID;
                            newAppointment.haircutID = tblAppointment.haircutID;
                            newAppointment.timeSlotID = tblAppointment.timeSlotID;
                        }
                        using (db)
                        {
                            
                            db.tblAppointments.Add(newAppointment);
                            db.SaveChanges();
                        }
                        int findAppointmentID = newAppointment.appointmentID;
                        return RedirectToAction("confirmationEmail", "Appointment", new { appointmentID = findAppointmentID });
                    }
                    catch (Exception)
                    {
                        TempData["appointmentError"] = "<p>Unable to make apppointment please try again later.</p>";
                    }
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
            if(TempData["userInfo"] != null)
            {
                TempData.Keep("userInfo");
            }

            if (tblAppointment == null)
            {
                return HttpNotFound();
            }
            return View(tblAppointment);
        }
        public ActionResult DownloadAppointmentCard(int? appointmentID)
        {
            tblAppointment tblAppointment = db.tblAppointments.Find(appointmentID);
            tblEmployee tblEmployee = db.tblEmployees.Find(tblAppointment.employeeID);
            tblHaircut tblHaircut = db.tblHaircuts.Find(tblAppointment.haircutID);
            tblTimeSlot tblTimeSlot = db.tblTimeSlots.Find(tblAppointment.timeSlotID);
            ViewData["employee"] = tblEmployee.FirstName + " " + tblEmployee.LastName;
            ViewData["haircut"] = tblHaircut.HaircutName;
            ViewData["timeSlot"] = tblTimeSlot.timeSlot;
            if(TempData["userInfo"] != null)
            {
                TempData.Keep("userInfo");
            }

            return new Rotativa.ViewAsPdf("appointmentConfirmation", tblAppointment)
            {
                FileName = "Mobile Hairdresser : Appointment Card.pdf",
                PageSize = Size.A4,
                PageOrientation = Orientation.Portrait,
                PageMargins = {Left = 0, Right = 0},
                CustomSwitches = "--print-media-type --zoom 1.3"
            };
        }
        public ActionResult confirmationEmail(int appointmentID)
        {
            string body;
            if(TempData["userInfo"] != null)
            {
                TempData.Keep("userInfo");
            }
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
                string clientPassword = TempData["userInfo"].ToString();
                int customerHouseNumber = Convert.ToInt32(tblAppointment.tblClient.clientHouseNumber);
                string customerAddress = tblAppointment.tblClient.clientPostalCode;
                string emailFrom = ConfigurationManager.AppSettings["EmailFormAddress"];

                MailMessage confirmationMessage = new MailMessage();
                confirmationMessage.To.Add(customerEmail);
                confirmationMessage.From = new MailAddress(emailFrom, "Mobile Hairdresser");
                confirmationMessage.ReplyToList.Add(new MailAddress(customerEmail));
                confirmationMessage.Subject = @"Mobile Hairdresser : Appointment Confirmation";
                confirmationMessage.Body = string.Format(body, customerName, customerPhoneNumber, customerEmail, clientPassword
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
                            TempData["appointmentError"] = "No appointments have been booked for this day!";
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
                            TempData["appointmentError"] = "No appointments have been book for this day!";
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
        [HttpGet]
        public ActionResult Search(int? appointmentID, string clientPassword)
        {
            if(appointmentID == null)
            {
                return View();
            }
            tblAppointment tblAppointment = db.tblAppointments.Find(appointmentID);
            try
            {
                string salt = tblAppointment.tblClient.salt;

                if ((Crypto.VerifyHashedPassword(tblAppointment.tblClient.Password, clientPassword + salt))||(Session["user"] != null))
                {
                    TempData["userInfo"] = tblAppointment.appointmentID;
                    tblEmployee tblEmployee = db.tblEmployees.Find(tblAppointment.employeeID);
                    tblHaircut tblHaircut = db.tblHaircuts.Find(tblAppointment.haircutID);
                    ViewData["timeSlot"] = tblAppointment.tblTimeSlot.timeSlot;
                    ViewData["employee"] = tblEmployee.FirstName + " " + tblEmployee.LastName;
                    ViewData["haircut"] = tblHaircut.HaircutName;
                    if ((tblAppointment == null) || (tblEmployee == null) || (tblHaircut == null))
                    {
                        return HttpNotFound();
                    }
                    return View(tblAppointment);
                }
            }
            catch(Exception)
            {
                ViewBag.searchError = "No appointment could be found please try again later!";
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
                if ((tblAppointment.appointmentDate - DateTime.Today).TotalDays <= 0)
                {
                    TempData["appointmentError"] = "Sorry we are unable to change the appointment on the same day."
                                                    + "Please contact Mobile Hairdresser's directly to make changes ";
                    return Redirect(Request.UrlReferrer.ToString());
                }
                else
                {
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
            }
            catch (Exception)
            {
                TempData["appointmentError"] = "<p>Unable to make apppointment please try again later.</p>";
            }
            return RedirectToAction("Search", "Appointment");
        }
        [HttpGet]
        public async Task<ActionResult> cancelAppointment(int? appointmentID)
        {
            try
            {
                tblAppointment tblAppointment = await db.tblAppointments.FindAsync(appointmentID);
                db.tblClients.Remove(tblAppointment.tblClient);
                db.tblAppointments.Remove(tblAppointment);
                await db.SaveChangesAsync();
            }
            catch(Exception)
            {
                ViewBag.searchError = "Your appointment could not be found please try again, or contact the Mobile Hairdresser";
            }
            return RedirectToAction("Search", "Appointment");
        }
        [HttpPost]
        public async Task<ActionResult> cancelAppointmentConfirmation(int? appointmentID)
        {
            tblAppointment tblAppointment = await db.tblAppointments.FindAsync(appointmentID);
            if (((tblAppointment.appointmentDate - DateTime.Today).TotalDays <= 0)||(Session["user"] == null))
            {
                ViewData["appointmentError"] = "We are unable to cancel any appointment on the same day."
                                                + "Please contact the Mobile Hairdresser's directly to cancel the appointment";
                return RedirectToAction(Request.UrlReferrer.ToString());
            }
            else
            {
                db.tblAppointments.Remove(tblAppointment);
                await db.SaveChangesAsync();
                return RedirectToAction("Search", "Appointment");
            }
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
            if (disposing)
            {
                db.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}