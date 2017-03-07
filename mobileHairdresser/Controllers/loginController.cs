using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using mobileHairdresser.Database;
using System.Web.Helpers;
using System.IO;
using System.Configuration;
using System.Net.Mail;
using System.Net;


namespace mobileHairdresser.Controllers
{
    public class loginController : BaseController
    {
        int LoginID;
        // GET: login
        public ActionResult createSession()
        {
            var fullname = "";
            //Creates Session by pulling the users name form the database and creating a session with those details.
            var NameSearch = (from t in db.tblEmployees
                              join id in db.tblLogins on t.LoginID equals id.loginID
                              where t.LoginID == LoginID
                              select new { t.FirstName, t.LastName, t.LoginID }).First();

            fullname = NameSearch.FirstName.ToString() + " " + NameSearch.LastName.ToString();


            //Create a session
            Session["user"] = fullname;
            Session["loginID"] = LoginID;

            return RedirectToAction("getAuthentication", "login");
        }
        [HttpPost]
        public ActionResult getAuthenticated(tblLogin loginDetails, tblEmployee userInfo, string ReturnUrl)
        {

            string username = loginDetails.Username.ToLower();
            string password = loginDetails.Password;
            bool verifyHash = false;
            string uname = "";
            string pword = "";
            string isFirstLogin = "";
            bool authenticateUser = false;

            try
            {
                var loginSearch = (from t in db.tblLogins
                                   where t.Username == username
                                   select new { t.Username, t.Password, t.loginID, t.IsDefault }).First();
                uname = loginSearch.Username;
                pword = loginSearch.Password;
                LoginID = loginSearch.loginID;
                isFirstLogin = loginSearch.IsDefault.ToString();

                switch (isFirstLogin)
                {
                    case "true":

                        verifyHash = Crypto.VerifyHashedPassword(pword, password);

                        if (verifyHash != true)
                        {
                            var checkPassword = db.tblLogins.Where(a => a.Username.Equals(username) && a.Password.Equals(password)).FirstOrDefault();
                            if (checkPassword != null)
                            {
                                authenticateUser = true;
                            }
                        }
                        else
                        {
                            if (username == uname && verifyHash == true)
                            {
                                authenticateUser = true;
                            }
                        }
                        break;
                    case "false":

                        verifyHash = Crypto.VerifyHashedPassword(pword, password);

                        if (username == uname && verifyHash == true)
                        {
                            authenticateUser = true;
                        }
                        else
                        {
                            authenticateUser = false;
                            TempData["loginErrorMessage"] = "The username or password entered did not match. Please check and try again!";
                        }
                        break;
                }

                if (authenticateUser == true)
                {
                    createSession();
                    if (isFirstLogin == "true")
                    {
                        return RedirectToAction("changePassword", "login");
                    }
                    else
                    {
                        TempData["systemMessage"] = "<h1>Login has been succesfull.</h1>";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch (Exception)
            {
                    TempData["systemMessage"] = "<p>Unable to login at this time please try again later.</p>";
            }
            return RedirectToAction("Index", "Home");
        }
        public ActionResult changePassword()
        {
            if (Session["user"] != null && Session["LoginID"] != null)
            {
                var loginID = Session["loginID"];
                return View(db.tblLogins.Find(loginID));
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public ActionResult changePassword(tblLogin changePassword)
        {
            string currentPassword = changePassword.Password.ToString();
            string newPassword = Request["newPassword"];
            string confirmPassword = Request["confirmPassword"];
            bool verifyHash = false;
            string isFirstLogin = "";
            bool updatePassword = false;
            var hashedPassword = "";
            int username = Convert.ToInt32(Session["loginID"]);

            if (confirmPassword.Length <= 7)
            {
                ViewBag.errorMsg = "The password you entered is not long enough, Please ensure your password long than 6 characters";
            }
            else
            {
                var passwordSearch = (from t in db.tblLogins
                                      where t.loginID == username
                                      select new { t.Password, t.IsDefault }).First();
                isFirstLogin = passwordSearch.IsDefault;
                hashedPassword = passwordSearch.Password;


                switch (isFirstLogin)
                {
                    case "true":

                        verifyHash = Crypto.VerifyHashedPassword(hashedPassword, currentPassword);

                        if (verifyHash != true)
                        {
                            var checkPassword = db.tblLogins.Where(a => a.Password.Equals(currentPassword)).FirstOrDefault();
                            if (checkPassword != null)
                            {
                                updatePassword = true;
                            }
                        }
                        else
                        {
                            if (verifyHash == true)
                            {
                                updatePassword = true;
                            }
                        }
                        break;
                    case "false":

                         verifyHash = Crypto.VerifyHashedPassword(hashedPassword, currentPassword);

                        if (verifyHash == true)
                        {
                            updatePassword = true;
                        }
                        break;
                }


                if (updatePassword != true)
                {
                    ViewBag.errorMsg = "Incorrect password entered please try again!";
                    return View();
                }
                else
                {
                    if (newPassword == confirmPassword)
                    {
                        if (updatePassword == true)
                        {
                            tblLogin findUser = db.tblLogins.Find(changePassword.loginID);

                            findUser.Password = Crypto.HashPassword(newPassword).ToString();

                            if (findUser.IsDefault == "true")
                            {
                                findUser.IsDefault = "false";
                            }

                            ViewBag.errorMsg = "Your password has been successfully changed!";

                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        ViewBag.errorMsg = "New password dose not match please check and try again!";
                        return View();
                    }
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session["user"] = null;
            Session["loginID"] = null;

            TempData["systemMessage"] = "<h2>You have successfully been logged out</h2>";

            return RedirectToAction("Index", "Home");
        }
        public ActionResult recoverPassword()
        {

            return View();
        }

        [HttpPost]
        public ActionResult recoverPassword(tblLogin username)
        {
            string searchUsername = Request["userName"];

            try
            {
                string strPwdchar = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                string strPwd = "";

                var userSeacrh = (from t in db.tblEmployees
                                  join id in db.tblLogins on t.LoginID equals id.loginID
                                  where id.Username == searchUsername
                                  select new { id.loginID, id.Username, id.Password, id.IsDefault, t.Email, t.FirstName, t.LastName }).First();

                //Create a randomised password
                Random rnd = new Random();
                for (int i = 0; i <= 8; i++)
                {
                    int iRandom = rnd.Next(0, strPwdchar.Length - 1);
                    strPwd += strPwdchar.Substring(iRandom, 1);
                }

                //Hashed newly created random password
                var hashedNewPassword = Crypto.HashPassword(strPwd).ToString();

                //Save new password to database
                try
                {
                    tblLogin findUser = db.tblLogins.Find(userSeacrh.loginID);

                    findUser.Password = hashedNewPassword.ToString();
                    //Sets the database to ask for user to change password when next logged in
                    findUser.IsDefault = "true";

                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    ViewBag.error = ex;
                }
                

                //ViewBag.error = "New password was created and saved to the database.";

                string emailName = HttpUtility.UrlEncode(userSeacrh.FirstName + userSeacrh.LastName);
                string userName = HttpUtility.UrlEncode(userSeacrh.Username);
                string emailPassword = HttpUtility.UrlEncode(strPwd);

                try
                {
                    string body = "";

                    using (var sr = new StreamReader(Server.MapPath("//App_Data//emailTemplates/") + "recoveryPassword.html"))
                    {
                        body = sr.ReadToEnd();
                    }

                    string senderEmail = ConfigurationManager.AppSettings["EmailFormAddress"];
                    string senderUsername = ConfigurationManager.AppSettings["MailAuthUser"];
                    string senderPassword = ConfigurationManager.AppSettings["MailAuthPass"];
                    string senderDisplayName = "Mobile Hairdresser : Password Recovery";

                    MailMessage passwordRecoveryMessage = new MailMessage();
                    passwordRecoveryMessage.To.Add(userSeacrh.Email);
                    passwordRecoveryMessage.From = new MailAddress(senderEmail, senderDisplayName);
                    passwordRecoveryMessage.Subject = @"" + senderDisplayName;
                    passwordRecoveryMessage.Body = string.Format(body, emailName, emailName, emailPassword);
                    passwordRecoveryMessage.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient())
                    {
                        smtp.EnableSsl = true;
                        smtp.Host = "smtp.gmail.com";
                        smtp.Port = 587;
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = new NetworkCredential(senderUsername, senderPassword);
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.SendCompleted += (s, e) => { smtp.Dispose(); };
                        smtp.Send(passwordRecoveryMessage);
                    }
                    ViewBag.error = "Your new password has been sent to " + userSeacrh.Email;
                }
                catch(Exception error)
                {
                    ViewBag.error = "An error has occurred and you email has not been sent! : " + error ; 
                }        
            }
            catch(Exception error)
            {
                    ViewBag.error = "New password was not created : " + error;
            }

            return View();
        }
    }
}