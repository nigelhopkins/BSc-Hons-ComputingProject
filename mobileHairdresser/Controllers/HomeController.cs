using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using mobileHairdresser.Database;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Configuration;
using System.Net;

namespace mobileHairdresser.Controllers
{
    public class HomeController : BaseController
    {
        
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

            mobileHairdresserEntities db = new mobileHairdresserEntities();
            List<tblGallery> gallery = db.tblGalleries.ToList();

            return View(gallery);
        }
        public ActionResult Contact()
        {
            ViewBag.Title = "Contact";
            
                return View();
        }
        [HttpPost]
        public ActionResult sendEmail()
        {
            string body = "";
            bool sendToCustomer = false;
            string emailTo = "";
            string custEmail = "";
            string custName = "";
            string custPhoneNo = "";
            string emailMessage = "";
            bool emailSaved = false;
            int count = 0;
            do
            {
                if (sendToCustomer != true)
                {
                    emailTo = ConfigurationManager.AppSettings["EmailFormAddress"];
                    custEmail = Request["custEmail"];
                }
                else
                {
                    emailTo = Request["custEmail"];
                    custEmail = ConfigurationManager.AppSettings["EmailFormAddress"];
                }

                int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
                string username = ConfigurationManager.AppSettings["MailAuthUser"];
                string password = ConfigurationManager.AppSettings["MailAuthPass"];
                custName = Request["custName"];
                custPhoneNo = Request["custPhoneNo"];
                emailMessage = Request["emailMessage"];

                if(emailSaved != true)
                {
                    try
                    {
                        tblEmail newEmail = new tblEmail();
                        newEmail.custEmail = custEmail;
                        newEmail.CustName = custName;
                        newEmail.custPhone = custPhoneNo;
                        newEmail.Subject = "You have recived a new message from : " + custName;
                        newEmail.Message = emailMessage;
                        newEmail.Read = "false";
                        newEmail.DateSent = DateTime.Now;

                        db.tblEmails.Add(newEmail);
                        db.SaveChanges();

                        emailSaved = true;
                    }
                    catch (Exception error)
                    {
                        TempData["error"] = "Email sent but not added to database.";
                    }
                }
                
                try
                {

                    if (sendToCustomer != true)
                    {
                        using (var sr = new StreamReader(Server.MapPath("//App_Data//emailTemplates/" + "contactEmailNotification.html")))
                        {
                            body = sr.ReadToEnd();
                        }
                    }
                    else
                    {
                        using (var sr = new StreamReader(Server.MapPath("//App_Data//emailTemplates/" + "customerEmailNotification.html")))
                        {
                            body = sr.ReadToEnd();
                        }
                    }

                    MailMessage customerEmailMessage = new MailMessage();
                    customerEmailMessage.To.Add(emailTo);
                    customerEmailMessage.From = new MailAddress(emailTo, "Mobile Hairdresser");
                    customerEmailMessage.ReplyTo = new MailAddress(custEmail);
                    customerEmailMessage.Subject = @"You have recived a new message from : " + custName;
                    if (sendToCustomer != true)
                    {
                        customerEmailMessage.Body = string.Format(body, custName, custPhoneNo, custEmail, emailMessage);
                    }
                    else
                    {
                        customerEmailMessage.Body = string.Format(body, custName, custPhoneNo, emailTo, emailMessage);
                    }

                    customerEmailMessage.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient())
                    {
                        smtp.EnableSsl = true;
                        smtp.Host = ConfigurationManager.AppSettings["MailServer"];
                        smtp.Port = smtpPort;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(username, password);
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.SendCompleted += (s, e) => { smtp.Dispose(); };
                        smtp.Send(customerEmailMessage);
                    }

                    sendToCustomer = true;
                }
                catch (Exception error)
                {
                    TempData["error"] = "An error has occured and your email could not be sent! : " + error;

                    return RedirectToAction("Contact", "Home");
                }
                count++;
            } while (count == 1);                              

            TempData["EmailConfirmation"] = custName;
                return RedirectToAction("Contact", "Home");
          

            return RedirectToAction("Contact", "Home");
        }
        public ActionResult cookiePolicy()
        {
            ViewBag.Title = "Our Cookie Policy";
            return View();
        }
    }
}
