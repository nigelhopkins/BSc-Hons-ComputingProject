using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using mobileHairdresser.Database;
using System.Linq;

namespace mobileHairdresser.Controllers
{
    public class adminController : BaseController
    {
        // GET: admin
        public async Task<ActionResult> adminAccountList()
        {
            if (Session["user"] != null && Session["loginID"] != null)
            {
                var tblEmployees = db.tblEmployees.Include(t => t.tblLogin);
                return View(await tblEmployees.ToListAsync());
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: admin/Details/5
        public async Task<ActionResult> ViewAdminAccountDetails(int? id)
        {
            if(Session["user"] != null && Session["loginID"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tblEmployee tblEmployee = await db.tblEmployees.FindAsync(id);

                if (tblEmployee == null)
                {
                    return HttpNotFound();
                }
                return View(tblEmployee);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        // GET: admin/Create
        public ActionResult createNewAdminAccount()
        {
            if(Session["user"] != null && Session["loginID"] != null)
            {
                ViewBag.LoginID = new SelectList(db.tblLogins, "loginID", "Username");
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            
        }

        // POST: admin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        public ActionResult createNewAdminAccount(tblEmployee tblEmployee, tblLogin tblLogin)
        {
            if(Session["user"] != null && Session["loginID"] != null)
            {

                bool findEmployee = db.tblEmployees.Where(x => x.FirstName.Equals(tblEmployee.FirstName) && x.LastName.Equals(tblEmployee.LastName)).Any();

                if(findEmployee)
                {
                    RedirectToAction("Index", "Home");
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        tblLogin newLogin = new tblLogin();
                        {
                            newLogin.Password = tblEmployee.tblLogin.Password;
                            newLogin.IsDefault = "true";
                        }
                        db.tblLogins.Add(newLogin);
                        db.SaveChanges();

                        tblEmployee newEmployee = new tblEmployee();
                        {
                            newEmployee.LoginID = newLogin.loginID;
                            newEmployee.FirstName = tblEmployee.FirstName;
                            newEmployee.LastName = tblEmployee.LastName;
                            newEmployee.PhoneNumber = tblEmployee.PhoneNumber;
                            newEmployee.Email = tblEmployee.Email;
                            
                        }
                        using (db)
                        {
                            db.tblEmployees.Add(newEmployee);
                            db.SaveChanges();
                        }
                        return RedirectToAction("adminAccountList");
                    }
                    
                    return View(tblEmployee);
                }
            }
            else
            {
                RedirectToAction("Index", "Home");
            }
            return View();
        }

        // GET: admin/Edit/5
        public async Task<ActionResult> EditAdminAccount(int? id)
        {
            if(Session["user"] != null && Session["loginID"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tblEmployee tblEmployee = await db.tblEmployees.FindAsync(id);
                if (tblEmployee == null)
                {
                    return HttpNotFound();
                }
                return View(tblEmployee);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
           
        }

        // POST: admin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAdminAccount([Bind(Include = "employeeID,FirstName,LastName,Email,PhoneNumber,LoginID")] tblEmployee tblEmployee)
        {
            if(Session["user"] != null && Session["loginID"] != null)
            {
                if (ModelState.IsValid)
                {
                    db.Entry(tblEmployee).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("adminAccountList");
                }
                return View(tblEmployee);
            }
            else
            {
                RedirectToAction("Index", "Home");
            }

            return View();
        }

        // GET: admin/Delete/5
        public async Task<ActionResult> DeleteAdminAccount(int? id)
        {
            if(Session["user"] != null && Session["loginID"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tblEmployee tblEmployee = await db.tblEmployees.FindAsync(id);
                if (tblEmployee == null)
                {
                    return HttpNotFound();
                }
                return View(tblEmployee);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: admin/Delete/5
        [HttpPost, ActionName("DeleteAdminAccount")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            if(Session["user"] != null && Session["loginID"] != null)
            {
                tblEmployee tblEmployee = await db.tblEmployees.FindAsync(id);
                db.tblEmployees.Remove(tblEmployee);
                await db.SaveChangesAsync();
                return RedirectToAction("adminAccountList");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if(Session["user"] != null && Session["loginID"] != null)
            {
                if (disposing)
                {
                    db.Dispose();
                }
                base.Dispose(disposing);

            }           
        }
    }
}
