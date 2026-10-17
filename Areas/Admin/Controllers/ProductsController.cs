using _25dh110213_LTW.Models;
using _25dh110213_LTW.Models.ViewModel;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace _25dh110213_LTW.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // Lưu ảnh upload vào Content/images và ghi đường dẫn vào ProductImage
        private void SaveImage(Product product)
        {
            if (product.UploadImg != null && product.UploadImg.ContentLength > 0)
            {
                string filename = Path.GetFileName(product.UploadImg.FileName).Replace(" ", "_");
                string folder = Server.MapPath("~/Content/images/");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                product.UploadImg.SaveAs(Path.Combine(folder, filename));
                product.ProductImage = "~/Content/images/" + filename;
            }
        }

        // GET: Admin/Products  (tìm kiếm, lọc giá, sắp xếp, phân trang)
        public ActionResult Index(string searchTerm, decimal? minPrice, decimal? maxPrice, string sortOrder, int? page)
        {
            var model = new SearchProductVM();
            var products = db.Products.Include(p => p.Category).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm)
                                            || p.ProductDescription.Contains(searchTerm)
                                            || p.Category.CategoryName.Contains(searchTerm));
            }
            if (minPrice.HasValue)
            {
                model.MinPrice = minPrice;
                products = products.Where(p => p.ProductPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                model.MaxPrice = maxPrice;
                products = products.Where(p => p.ProductPrice <= maxPrice.Value);
            }

            model.SortOrder = sortOrder;
            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.ProductName);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.ProductName);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.ProductPrice);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.ProductPrice);
                    break;
                default:
                    products = products.OrderBy(p => p.ProductID);
                    break;
            }

            model.PageNumber = page ?? 1;
            model.PageSize = 5;
            model.Products = products.ToPagedList(model.PageNumber, model.PageSize);
            return View(model);
        }

        // GET: Admin/Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Admin/Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,CategoryID,ProductName,ProductPrice,ProductImage,ProductDescription,UploadImg")] Product product)
        {
            if (ModelState.IsValid)
            {
                SaveImage(product);
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Admin/Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductID,CategoryID,ProductName,ProductPrice,ProductImage,ProductDescription,UploadImg")] Product product)
        {
            if (ModelState.IsValid)
            {
                SaveImage(product);
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Admin/Products/Delete/5  (dùng lại code của Details)
        public ActionResult Delete(int? id)
        {
            return Details(id);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            try
            {
                db.Products.Remove(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (DbUpdateException)
            {
                // Sản phẩm đã nằm trong đơn hàng thì không xóa được
                db.Entry(product).State = EntityState.Unchanged;
                ViewBag.Error = "Không thể xóa sản phẩm này vì đã có trong đơn hàng.";
                return View("Delete", product);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
