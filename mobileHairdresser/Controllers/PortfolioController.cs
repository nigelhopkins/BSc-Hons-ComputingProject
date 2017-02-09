using mobileHairdresser.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace mobileHairdresser.Controllers
{
    public class PortfolioController : Controller
    {
        public mobileHairdresserEntities db = new mobileHairdresserEntities();
        public string fileLocation = "~/Content/Images/Portfolio/";
        // GET: Portfolio
        [HttpGet]
        public ActionResult PhotoGallery(int GalleryID)
        {
            List<tblPhoto> getPhotos = db.tblPhotos.Where(gallery => gallery.GalleryID == GalleryID).ToList();

            var GalleryName = (from t in db.tblGalleries
                               where t.GalleryID == GalleryID
                               select new { t.GalleryID, t.GalleryName, t.GalleryDiscription }).FirstOrDefault();

            ViewBag.GalleryName = GalleryName.GalleryName;
            ViewBag.GalleryID = GalleryName.GalleryID;
            ViewBag.GalleryDiscription = GalleryName.GalleryDiscription;

            if (getPhotos.Count != 0)
            {
                return View(getPhotos);
            }
            else
            {
                TempData["NoPhotosFound"] = "There are no photos in this gallery.";
                return View();
            }
        }
        public async Task<ActionResult> addPhotos(int GalleryID)
        {
            if(Session["user"] != null)
            {
                ViewBag.GalleryID = new SelectList(db.tblGalleries, "GalleryID", "GalleryName");
                TempData["getGalleryID"] = GalleryID;
                return View();
            }
            else
            {
                return RedirectToAction("Porfolio", "Home");
            }
        }

        [HttpPost]
        public ActionResult uploadPhotos(int galleryID)
        {
            if(Session["user"] != null)
            {                
                    /******* 
                    * upload script supplied from the following web site : 
                    * http://www.c-sharpcorner.com/article/asp-net-mvc-drag-drop-multiple-image-upload/            
                    ******/

                    bool isSavedSuccessfully = true;
                    bool isAddedSucessfully = true;
                    string fName = "";
                    string fNameWithoutExtension = "";

                   

                    try
                    {
                    
                    foreach (string fileName in Request.Files)
                        {
                            HttpPostedFileBase file = Request.Files[fileName];
                            fName = file.FileName;
                        
                            if (file != null && file.ContentLength > 0)
                            {

                                var fileName1 = Path.GetFileName(file.FileName);
                                fNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);

                                var path = Path.Combine(Server.MapPath(fileLocation));
                                string pathString = System.IO.Path.Combine(path.ToString());

                                bool isExists = System.IO.Directory.Exists(pathString);
                                if (!isExists) 
                                    {
                                            System.IO.Directory.CreateDirectory(pathString);

                                    }

                            var uploadpath = string.Format("{0}\\{1}", pathString, file.FileName);
                            file.SaveAs(uploadpath);
                            try
                            {
                                tblPhoto newPhoto = new tblPhoto();
                                newPhoto.PhotoName = fNameWithoutExtension;
                                newPhoto.Description = fNameWithoutExtension;
                                newPhoto.Url = fName;
                                newPhoto.DateStamp = DateTime.Now;
                                newPhoto.GalleryID = galleryID;

                                db.tblPhotos.Add(newPhoto);

                                db.SaveChangesAsync();

                            }
                            catch(Exception ex)
                            {
                                isAddedSucessfully = false;
                            }
                            if(isAddedSucessfully)
                            {
                                return Json(new
                                {
                                    Message = fName
                                });
                            }
                            else
                            {
                                return Json(new
                                {
                                    Message = "Error in adding to the database"
                                });
                            }
                        }
                        }
                    }
                    catch (Exception ex)
                    {
                        isSavedSuccessfully = false;
                    }
                    if (isSavedSuccessfully)
                    {
                        return Json(new
                        {
                            Message = fName
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            Message = "Error in saving file"
                        });
                    }
                    
                return RedirectToAction("PhotoGallery", "Portfolio");
            }
            else
            {
                RedirectToAction("Portfolio", "Home");
            }
            return View();
        }
        public ActionResult CreateGallery()
        {
            if(Session["user"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Portfolio", "Home");
            }
        }
        public async Task<ActionResult> EditGallery(int? GalleryID)
        {
            if(Session["user"] != null)
            {
                if(GalleryID == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                tblGallery tblGallery = await db.tblGalleries.FindAsync(GalleryID);

                ViewBag.GalleryIDList = new SelectList(db.tblGalleries, "GalleryID", "GalleryName");
                if (tblGallery == null)
                {
                    return HttpNotFound();
                }
                return View(tblGallery);
            }
            else
            {
                return RedirectToAction("Portfolio", "Home");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveGallery(int? GalleryID, string GalleryName, string GalleryImg, [Bind(Include = "GalleryID, GalleryName,GalleryImg, GalleryDiscription")] tblGallery tblGallery)
        {
            if(Session["user"] != null)
            {
                GalleryImg = null;

                if(ModelState.IsValid)
                {
                    bool saveData = db.tblGalleries.Any(record => record.GalleryID == GalleryID);

                    if(saveData)
                    {
                        db.Entry(tblGallery).State = EntityState.Modified;
                    }
                    else
                    {
                        
                        db.tblGalleries.Add(tblGallery);
                    }
                    await db.SaveChangesAsync();
                    return RedirectToAction("Portfolio", "Home");
                }

                return View(tblGallery);
            }
            else
            {
                RedirectToAction("Portfolio", "Home");
            }
            return View();
        }
        [HttpGet]
        public async Task<ActionResult> DeleteGallery(int? GalleryID)
        {
            if(Session["user"] != null)
            {
                if(GalleryID == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tblGallery tblGallery = await db.tblGalleries.FindAsync(GalleryID);

                if(tblGallery == null)
                {
                    return HttpNotFound();
                }
                return View(tblGallery);
            }
            else
            {
                return RedirectToAction("Portfolio", "Home");
            }
        }      
        public ActionResult setGalleryPhoto(int PhotoID)
        {
            if(Session["user"] != null)
            {
                if(ModelState.IsValid)
                {
                    var getPhoto = (from photo in db.tblPhotos
                                    join gallery in db.tblGalleries on photo.GalleryID equals gallery.GalleryID
                                    where photo.PhotoID == PhotoID
                                    select new { photo.Url, photo.GalleryID }).First();

                    var findGallery = db.tblGalleries.Find(getPhoto.GalleryID);                    

                    if(findGallery != null)
                    {
                        bool gallery = db.tblGalleries.Any(record => record.GalleryID == findGallery.GalleryID);

                        if(gallery)
                        {
                            findGallery.GalleryImg = getPhoto.Url;

                            db.Entry(findGallery).State = EntityState.Modified;
                            db.SaveChanges();
                        }

                        RedirectToAction("Portfolio", "Home");
                    }
                }
                else
                {
                    RedirectToAction("Portfolio", "Home");
                }
            }

            return RedirectToAction("Portfolio", "Home");
        }
        public async Task<ActionResult> editGalleryDetails(int? GalleryID)
        {
            if (Session["user"] != null)
            {
                if (GalleryID == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                tblGallery tblGallery = await db.tblGalleries.FindAsync(GalleryID);
                if (tblGallery == null)
                {
                    return HttpNotFound();
                }
                return View(tblGallery);
            }
            else
            {
                return RedirectToAction("Portfolio", "Home");
            }

            return RedirectToAction("Portfolio", "Home");
        }
        public ActionResult deletePhoto(int PhotoID)
        {
            if (Session["user"] != null)
            {

                var findPhoto = db.tblPhotos.Find(PhotoID);

                var fileName = findPhoto.Url;
                int GalleryID = findPhoto.GalleryID;

                string fullPath = Request.MapPath(fileLocation + fileName);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                    tblPhoto tblPhotos = db.tblPhotos.Find(PhotoID);

                    var photoID = tblPhotos.PhotoID;
                    db.tblPhotos.Remove(tblPhotos);
                }

                db.SaveChanges();

                return RedirectToAction("PhotoGallery", "Portfolio", new { GalleryID = GalleryID });
            }
            return RedirectToAction("Portfolio", "Home");
        }
        [HttpPost, ActionName("DeleteGallery")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteGallery(int GalleryID)
        {
            if (Session["user"] != null)
            {
                string fileName = "";

                List<tblPhoto> tblPhotos = db.tblPhotos.Where(gallery => gallery.GalleryID.Equals(GalleryID)).ToList();
                foreach (var file in tblPhotos)
                {
                    string fullPath = Request.MapPath(fileLocation + file.Url);

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                        tblPhoto tblPhoto = db.tblPhotos.Find(file.PhotoID);
                        db.tblPhotos.Remove(tblPhoto);                        
                    }
                }

                tblGallery tblGallery = await db.tblGalleries.FindAsync(GalleryID);
                db.tblGalleries.Remove(tblGallery);
                await db.SaveChangesAsync();
                return RedirectToAction("Portfolio", "Home");
            }
            else
            {
                return RedirectToAction("Portfolio", "Home");
            }
        }
    }
}