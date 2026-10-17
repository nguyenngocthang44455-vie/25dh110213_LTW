using _25dh110213_LTW.Models;
using PagedList;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace _25dh110213_LTW.Areas.Admin.Controllers
{
    public class CustomersController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // GET: Admin/Customers
        public ActionResult Index(string searchTerm, string sortOrder, int? page)
        {
            var customers = db.Customers.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                customers = customers.Where(c => c.CustomerName.Contains(searchTerm)
                                               || c.CustomerPhone.Contains(searchTerm)
                                               || c.CustomerEmail.Contains(searchTerm));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    customers = customers.OrderByDescending(c => c.CustomerName);
                    break;
                default:
                    customers = customers.OrderBy(c => c.CustomerName);
                    break;
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.SortOrder = sortOrder;
            return View(customers.ToPagedList(page ?? 1, 5));
        }

        // GET: Admin/Customers/Details/5 (kèm danh sách đơn hàng của khách này)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Customer customer = db.Customers.Find(id.Value);
            if (customer == null)
            {
                return HttpNotFound();
            }
            ViewBag.Orders = db.Orders.Where(o => o.CustomerID == id.Value)
                                      .OrderByDescending(o => o.OrderDate).ToList();
            return View(customer);
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
