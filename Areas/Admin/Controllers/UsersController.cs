using _25dh110213_LTW.Models;
using PagedList;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace _25dh110213_LTW.Areas.Admin.Controllers
{
    public class UsersController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        private void AddValidationErrors(DbEntityValidationException ex)
        {
            foreach (var entity in ex.EntityValidationErrors)
            {
                foreach (var err in entity.ValidationErrors)
                {
                    ModelState.AddModelError(err.PropertyName, err.ErrorMessage);
                }
            }
        }

        // GET: Admin/Users
        public ActionResult Index(string searchTerm, int? page)
        {
            var users = db.Users.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u => u.Username.Contains(searchTerm) || u.UserRole.Contains(searchTerm));
            }
            ViewBag.SearchTerm = searchTerm;
            return View(users.OrderBy(u => u.Username).ToPagedList(page ?? 1, 5));
        }

        // GET: Admin/Users/Details/abc
        public ActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Admin/Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Username,Password,UserRole")] User user)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Any(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                    return View(user);
                }
                try
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    AddValidationErrors(ex);
                }
            }
            return View(user);
        }

        // GET: Admin/Users/Edit/abc
        public ActionResult Edit(string id)
        {
            return Details(id);
        }

        // POST: Admin/Users/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Username,Password,UserRole")] User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    AddValidationErrors(ex);
                }
            }
            return View(user);
        }

        // GET: Admin/Users/Delete/abc
        public ActionResult Delete(string id)
        {
            return Details(id);
        }

        // POST: Admin/Users/Delete/abc
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            User user = db.Users.Find(id);
            if (user != null)
            {
                db.Users.Remove(user);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
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
