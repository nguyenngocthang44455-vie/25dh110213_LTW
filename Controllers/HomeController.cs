using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PagedList;
using _25dh110213_LTW.Models;
using _25dh110213_LTW.Models.ViewModel;

namespace _25dh110213_LTW.Controllers
{
    public class HomeController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // GET: Home/Index
        public ActionResult Index(string searchTerm, int? page)
        {
            var model = new HomeProductVM();
            var products = db.Products.AsQueryable();

            // 1. Xử lý tìm kiếm sản phẩm theo tên, mô tả hoặc danh mục
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                products = products.Where(p => p.ProductName.Contains(searchTerm) ||
                                               p.ProductDescription.Contains(searchTerm) ||
                                               p.Category.CategoryName.Contains(searchTerm));
            }

            int pageNumber = page ?? 1;
            int pageSize = 6; // Hoặc 20 tùy theo yêu cầu (slide đề xuất phân trang sản phẩm mới)

            // 2. Lấy danh sách 10 sản phẩm bán chạy nhất (dựa vào số lượng trong OrderDetails)
            model.FeaturedProducts = products.OrderByDescending(p => p.OrderDetails.Count()).Take(10).ToList();

            // 3. Lấy danh sách sản phẩm mới có phân trang
            model.NewProducts = products.OrderBy(p => p.ProductID).ToPagedList(pageNumber, pageSize);

            return View(model);
        }
        // Danh sách sản phẩm theo danh mục
        public ActionResult ProductList(int? id, string sortOrder, int? page)
        {
            var query = db.Products.Include(p => p.Category).AsQueryable();

            if (id.HasValue)
            {
                var category = db.Categories.Find(id.Value);
                if (category == null)
                {
                    return HttpNotFound();
                }
                ViewBag.CategoryName = category.CategoryName;
                query = query.Where(p => p.CategoryID == id.Value);
            }
            else
            {
                ViewBag.CategoryName = "Tất cả sản phẩm";
            }

            switch (sortOrder)
            {
                case "name_asc":
                    query = query.OrderBy(p => p.ProductName);
                    break;
                case "price_asc":
                    query = query.OrderBy(p => p.ProductPrice);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.ProductPrice);
                    break;
                default:
                    query = query.OrderByDescending(p => p.ProductID);
                    break;
            }

            ViewBag.CategoryID = id;
            ViewBag.SortOrder = sortOrder;
            return View(query.ToPagedList(page ?? 1, 6));
        }

        // Chi tiết sản phẩm: id, quantity (số lượng muốn mua), page (phân trang phần bán chạy)
        // GET: Home/ProductDetails/5
        public ActionResult ProductDetails(int? id, int? quantity, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            Product pro = db.Products.Find(id);
            if (pro == null)
            {
                return HttpNotFound();
            }
            
            // Lấy tất cả các sản phẩm cùng danh mục (trừ sản phẩm hiện tại)
            var products = db.Products.Where(p => p.CategoryID == pro.CategoryID && p.ProductID != pro.ProductID).AsQueryable();
            ProductDetailsVM model = new ProductDetailsVM();

            int pageNumber = page ?? 1;
            int pageSize = model.PageSize;

            model.product = pro;
            model.RelatedProducts = products.OrderBy(p => p.ProductID).Take(8).ToList();
            model.TopProducts = products.OrderByDescending(p => p.OrderDetails.Count()).Take(8).ToPagedList(pageNumber, pageSize);

            if (quantity.HasValue)
            {
                model.quantity = quantity.Value;
            }

            return View(model);
        }
        // Compatibility route: forward singular ProductDetail to ProductDetails
        public ActionResult ProductDetail(int? id, int? quantity, int? page)
        {
            return ProductDetails(id, quantity, page);
        }
        // Menu danh mục (partial view dùng trong _CustomerLayout)
        [ChildActionOnly]
        public ActionResult CategoriesPV()
        {
            var categories = db.Categories.ToList();
            return PartialView("_CategoriesPV", categories);
        }
        // Xem chi tiết sản phẩm
        public ActionResult Details(int id)
        {
            var product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
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
