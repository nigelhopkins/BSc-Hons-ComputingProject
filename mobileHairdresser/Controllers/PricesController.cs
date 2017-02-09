using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using mobileHairdresser.Database;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Net;

namespace mobileHairdresser.Controllers
{
    public class PricesController : Controller
    {
        public mobileHairdresserEntities db = new mobileHairdresserEntities();
        // GET: Prices
        public ActionResult Index()
        {
            if(Session["user"] != null)
            {
                List<tblHaircutType> haircutType = db.tblHaircutTypes.ToList();
                ViewBag.Title = "Admin Prices";

                return View(haircutType);
            }
            else
            {
                Response.Redirect("~/Home/Index");
            }

            return View();
        }
        public ActionResult CreateNewHaircutType()
        {
            if(Session["user"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult AddNewHaircut()
        {
            if(Session["user"] != null)
            {
                ViewBag.TypeID = new SelectList(db.tblHaircutTypes, "TypeID", "TypeName");
                return View();  
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        public async Task<ActionResult> EditHaircutType(int? TypeID)
        {
            if (Session["user"] != null)
            {
                if (TypeID == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tblHaircutType tblHaircutType = await db.tblHaircutTypes.FindAsync(TypeID);

                ViewBag.TypeIDList = new SelectList(db.tblHaircutTypes, "TypeID", "TypeName");

                if (tblHaircutType == null)
                {
                    return HttpNotFound();
                }
                return View(tblHaircutType);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveHaircutsCatagory(int? TypeID, string TypeName, [Bind(Include = "TypeID, TypeName")] tblHaircutType tblHaircutType)
        {
            if (Session["user"] != null)
            {
                if (ModelState.IsValid)
                {
                    bool data = db.tblHaircutTypes.Any(record => record.TypeID == TypeID);

                    if (data)
                    {
                        db.Entry(tblHaircutType).State = EntityState.Modified;
                    }
                    else
                    {
                        db.tblHaircutTypes.Add(tblHaircutType);
                    }
                    await db.SaveChangesAsync();
                    return RedirectToAction("HaircutTypeDetails", "Prices", new { haircutTypeID = tblHaircutType.TypeID });
                }
                return View(tblHaircutType);
            }
            else
            {
                RedirectToAction("Index", "Home");
            }

            return View();
        }
        public async Task<ActionResult> DeleteHaircutsCatagory(int? id)
        {
            if (Session["user"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tblHaircutType tblHaircutType = await db.tblHaircutTypes.FindAsync(id);

                if (tblHaircutType == null)
                {
                    return HttpNotFound();
                }
                return View(tblHaircutType);
            }
            else
            {
                return RedirectToAction("HaircutTypeDetails", "Price");
            }
        }
        [HttpPost, ActionName("DeleteHaircutsCatagory")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteHaircutsCatagory(int id)
        {
            if (Session["user"] != null)
            {

                db.tblHaircuts.Where(p => p.TypeID.Equals(id)).ToList()
                .ForEach(p => db.tblHaircuts.Remove(p));
                
                tblHaircutType tblHaircutType = await db.tblHaircutTypes.FindAsync(id);
                db.tblHaircutTypes.Remove(tblHaircutType);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Prices");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpGet]
        public ActionResult HaircutTypeDetails(int haircutTypeID)
        {
            if (Session["user"] != null)
            {
                List<tblHaircut> haircutStyle = db.tblHaircuts.Where(hair => hair.TypeID == haircutTypeID).ToList();
                

                var TypeName = (from t in db.tblHaircutTypes
                                where t.TypeID == haircutTypeID
                                select new { t.TypeName, t.TypeID }).FirstOrDefault();

                ViewBag.PriceTypeName = TypeName.TypeName;
                ViewBag.PriceTypeID = TypeName.TypeID;
                
                return View(haircutStyle);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [HttpPost]
        public ActionResult HaircutTypeDetails(int? haircutID)
        {
            TempData["errorMessage"] = haircutID.ToString();

            var TypeName = (from t in db.tblHaircutTypes
                               where t.TypeID == haircutID
                               select new { t.TypeID, t.TypeName }).FirstOrDefault();

            ViewBag.PriceTypeName = TypeName.TypeName;
            ViewBag.PriceTypeID = TypeName.TypeID;

            return View();
        }
        public async Task<ActionResult> EditHaircuts(int? id)
        {
            if(Session["user"] != null)
            {
                if( id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tblHaircut tblHaircut = await db.tblHaircuts.FindAsync(id);

                ViewBag.TypeIDList = new SelectList(db.tblHaircutTypes, "TypeID" , "TypeName");

                if (tblHaircut == null)
                {
                    return HttpNotFound();
                }
                return View(tblHaircut);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }         
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveHaircuts(int? HaircutID, string HaircutName, [Bind(Include = "HaircutID,HaircutName,ShortPrice,LongPrice,TimeToCompletion,TypeID")] tblHaircut tblHaircut)
        {
            if (Session["user"] != null)
            {       
                if (ModelState.IsValid)
                {
                    bool data = db.tblHaircuts.Any(record => record.HaircutID == HaircutID);
                    

                    if (data)
                    {
                        db.Entry(tblHaircut).State = EntityState.Modified;
                    }
                    else
                    {
                        db.tblHaircuts.Add(tblHaircut);    
                    }

                    await db.SaveChangesAsync();
                    return RedirectToAction("HaircutTypeDetails", "Prices", new { haircutTypeID = tblHaircut.TypeID });
                }
                return View(tblHaircut);
            }
            else
            {
                RedirectToAction("Index", "Home");
            }

            return View();
        }
        public async Task<ActionResult> DeleteHaircut(int? id)
        {
            if(Session["user"] != null)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tblHaircut tblHaircut = await db.tblHaircuts.FindAsync(id);

                if (tblHaircut == null)
                {
                    return HttpNotFound();
                }
                return View(tblHaircut);
            }
            else
            {
                return RedirectToAction("HaircutTypeDetails", "Price");
            }
        }
        [HttpPost, ActionName("DeleteHaircut")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> HaircutDeleteConfirmed(int id)
        {
            if (Session["user"] != null)
            {
                tblHaircut tblHaircut = await db.tblHaircuts.FindAsync(id);
                var typeID = tblHaircut.TypeID;
                db.tblHaircuts.Remove(tblHaircut);
                await db.SaveChangesAsync();
                return RedirectToAction("HaircutTypeDetails", "Prices", new { haircutTypeID = typeID });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        protected override void Dispose(bool disposing)
        {
            if(Session["user"] != null)
            {
                if(disposing)
                {
                    db.Dispose();
                }
                base.Dispose(disposing);
            }
        }
    }
}