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
        public ActionResult createSession(string username)
        {
            //Creates Session by pulling the users name form the database and creating a session with those details.
            var NameSearch = (from t in db.tblEmployees
                              join id in db.tblLogins on t.LoginID equals id.loginID
                              where t.Email == username
                              select new { t.FirstName, t.LastName, t.LoginID }).First();

            //Create a session
            Session["user"] = NameSearch.FirstName.ToString() + " " + NameSearch.LastName.ToString();
            Session["loginID"] = LoginID;

            return RedirectToAction("getAuthentication", "login");
        }
        [HttpPost]
        public ActionResult getAuthenticated(tblEmployee userInfo, string ReturnUrl)
        {

            string username = userInfo.Email.ToLower();
            string password = userInfo.tblLogin.Password;
            bool verifyHash = false;
            string uname = "";
            string pword = "";
            string isFirstLogin = "";
            var salt = "";
            var saltedPassword = "";
            bool authenticateUser = false;

            try
            {
                var loginSearch = (from t in db.tblLogins
                                   join e in db.tblEmployees on t.loginID equals e.LoginID
                                   where e.Email == username
                                   select new { e.Email, t.Password, t.loginID, t.IsDefault, t.Salt }).First();
                uname = loginSearch.Email;
                pword = loginSearch.Password;
                salt = loginSearch.Salt;
                LoginID = loginSearch.loginID;
                isFirstLogin = loginSearch.IsDefault.ToString();
                saltedPassword = password + salt; 
                switch (isFirstLogin)
                {
                    case "true":

                        verifyHash = Crypto.VerifyHashedPassword(pword, saltedPassword);

                        if (verifyHash != true)
                        {
                            var checkPassword = db.tblEmployees.Where(a => a.Email.Equals(username) && a.tblLogin.Password.Equals(password)).FirstOrDefault();
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

                        verifyHash = Crypto.VerifyHashedPassword(pword, saltedPassword);

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
                    createSession(username);
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
            var saltedPassword = "";
            var generateSalt = "";
            var salt = "";
            int username = Convert.ToInt32(Session["loginID"]);

            if (confirmPassword.Length <= 7)
            {
                ViewBag.errorMsg = "The password you entered is not long enough, Please ensure your password long than 6 characters";
            }
            else
            {
                var passwordSearch = (from t in db.tblLogins
                                      where t.loginID == username
                                      select new { t.Password, t.IsDefault, t.Salt }).First();
                isFirstLogin = passwordSearch.IsDefault;
                salt = passwordSearch.Salt;
                hashedPassword = passwordSearch.Password;
                currentPassword = currentPassword + salt;


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

                            generateSalt = Crypto.GenerateSalt();
                            findUser.Salt = generateSalt;
                            saltedPassword = newPassword + generateSalt;                            
                            findUser.Password = Crypto.HashPassword(saltedPassword).ToString();

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
        public ActionResult recoverPassword(tblEmployee tblEmployee)
        {
            try
            {
                string strPwdchar = "abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                var salt = Crypto.GenerateSalt();
                var saltedPassword = "";
                string strPwd = "";

                var userSearch = (from t in db.tblEmployees
                                  join id in db.tblLogins on t.LoginID equals id.loginID
                                  where t.Email == tblEmployee.Email
                                  select new { id.loginID, id.Password, id.IsDefault, t.Email, t.FirstName, t.LastName }).First();

                //Create a randomised password
                Random rnd = new Random();
                for (int i = 0; i <= 8; i++)
                {
                    int iRandom = rnd.Next(0, strPwdchar.Length - 1);
                    strPwd += strPwdchar.Substring(iRandom, 1);
                }
                //Add salt to newly created password
                saltedPassword = strPwd + salt;
                //Hashed newly created random password
                var hashedNewPassword = Crypto.HashPassword(saltedPassword).ToString();

                //Save new password to database
                try
                {
                    tblLogin findUser = db.tblLogins.Find(userSearch.loginID);

                    findUser.Salt = salt.ToString();
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

                string emailName = userSearch.FirstName + " " + userSearch.LastName;
                string userName = userSearch.Email;
                string emailPassword = strPwd;

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
                    passwordRecoveryMessage.To.Add(userSearch.Email);
                    passwordRecoveryMessage.From = new MailAddress(senderEmail, senderDisplayName);
                    passwordRecoveryMessage.Subject = @"" + senderDisplayName;
                    passwordRecoveryMessage.Body = string.Format(body, emailName, userName, emailPassword);
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
                    ViewBag.error = "Your new password has been sent to " + userSearch.Email;
                }
                catch(Exception)
                {
                    ViewBag.error = "An error has occurred and you email has not been sent!" ; 
                }        
            }
            catch(Exception)
            {
                    ViewBag.error = "New password was not created!";
            }

            return View();
        }
    }
}