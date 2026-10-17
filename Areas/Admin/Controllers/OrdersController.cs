using _25dh110213_LTW.Models;
using PagedList;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace _25dh110213_LTW.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        private My_StoreEntities db = new   My_StoreEntities();

        // GET: Admin/Orders  (có thể lọc theo 1 khách: /Admin/Orders?customerId=1)
        public ActionResult Index(int? customerId, string sortOrder, int? page)
        {
            var orders = db.Orders.Include(o => o.Customer).AsQueryable();

            if (customerId.HasValue)
            {
                orders = orders.Where(o => o.CustomerID == customerId.Value);
                Customer c = db.Customers.Find(customerId.Value);
                ViewBag.CustomerName = c != null ? c.CustomerName : null;
            }

            switch (sortOrder)
            {
                case "date_asc":
                    orders = orders.OrderBy(o => o.OrderDate);
                    break;
                case "total_desc":
                    orders = orders.OrderByDescending(o => o.TotalAmount);
                    break;
                case "total_asc":
                    orders = orders.OrderBy(o => o.TotalAmount);
                    break;
                default:
                    orders = orders.OrderByDescending(o => o.OrderDate);
                    break;
            }

            ViewBag.CustomerId = customerId;
            ViewBag.SortOrder = sortOrder;
            return View(orders.ToPagedList(page ?? 1, 5));
        }

        // GET: Admin/Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Include(o => o.Customer)
                                   .Include(o => o.OrderDetails.Select(d => d.Product))
                                   .FirstOrDefault(o => o.OrderID == id.Value);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
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
