using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using mobileHairdresser.Database;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Net;

namespace mobileHairdresser.Controllers
{
    public class PricesController : Controller
    {
        public mobileHairdresserEntities priceListContext = new mobileHairdresserEntities();
        // GET: Prices
        public ActionResult Index()
        {
            if(Session["user"] != null)
            {
                mobileHairdresserEntities priceListContext = new mobileHairdresserEntities();
                List<tblHaircutType> haircutType = priceListContext.tblHaircutTypes.ToList();
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
                ViewBag.TypeID = new SelectList(priceListContext.tblHaircutTypes, "TypeID", "TypeName");
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
                tblHaircutType tblHaircutType = await priceListContext.tblHaircutTypes.FindAsync(TypeID);

                ViewBag.TypeIDList = new SelectList(priceListContext.tblHaircutTypes, "TypeID", "TypeName");

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
        public async Task<ActionResult> SaveHaircutsCatagory(int TypeID, [Bind(Include = "TypeID, TypeName")] tblHaircutType tblHaircutType)
        {
            if (Session["user"] != null)
            {
                if (ModelState.IsValid)
                {
                    bool data = priceListContext.tblHaircutTypes.Any(record => record.TypeID.Equals(TypeID));

                    if (data)
                    {
                        priceListContext.Entry(tblHaircutType).State = EntityState.Modified;
                        await priceListContext.SaveChangesAsync();
                        return RedirectToAction("HaircutTypeDetails", "Prices", new { haircutTypeID = tblHaircutType.TypeID });
                    }
                    else
                    {
                        priceListContext.tblHaircutTypes.Add(tblHaircutType);
                        await priceListContext.SaveChangesAsync();
                        return RedirectToAction("HaircutTypeDetails", "Prices", new { haircutTypeID = tblHaircutType.TypeID });
                    }
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
                tblHaircutType tblHaircutType = await priceListContext.tblHaircutTypes.FindAsync(id);

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

                priceListContext.tblHaircuts.Where(p => p.TypeID.Equals(id)).ToList()
                .ForEach(p => priceListContext.tblHaircuts.Remove(p));
                
                tblHaircutType tblHaircutType = await priceListContext.tblHaircutTypes.FindAsync(id);
                priceListContext.tblHaircutTypes.Remove(tblHaircutType);
                await priceListContext.SaveChangesAsync();
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
                List<tblHaircut> haircutStyle = priceListContext.tblHaircuts.Where(hair => hair.TypeID == haircutTypeID).ToList();
                

                var TypeName = (from t in priceListContext.tblHaircutTypes
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

            var TypeName = (from t in priceListContext.tblHaircutTypes
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
                tblHaircut tblHaircut = await priceListContext.tblHaircuts.FindAsync(id);

                ViewBag.TypeIDList = new SelectList(priceListContext.tblHaircutTypes, "TypeID" , "TypeName");

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
                    bool data = priceListContext.tblHaircuts.Any(record => record.HaircutID == HaircutID);
                    

                    if (data)
                    {
                        priceListContext.Entry(tblHaircut).State = EntityState.Modified;
                    }
                    else
                    {
                        priceListContext.tblHaircuts.Add(tblHaircut);    
                    }

                    await priceListContext.SaveChangesAsync();
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
                tblHaircut tblHaircut = await priceListContext.tblHaircuts.FindAsync(id);

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
                tblHaircut tblHaircut = await priceListContext.tblHaircuts.FindAsync(id);
                var typeID = tblHaircut.TypeID;
                priceListContext.tblHaircuts.Remove(tblHaircut);
                await priceListContext.SaveChangesAsync();
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
                    priceListContext.Dispose();
                }
                base.Dispose(disposing);
            }
        }
    }
}