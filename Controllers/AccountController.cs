using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using _25dh110213_LTW.Models;
using _25dh110213_LTW.Models.ViewModel;
using MaSV_MyStore.Models.ViewModel;

namespace _25dh110213_LTW.Controllers
{
    public class AccountController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        // 1. ĐĂNG KÝ (GET)
        public ActionResult Register()
        {
            return View();
        }
        // 1. ĐĂNG KÝ (POST)
        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = db.Users.SingleOrDefault(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                    return View(model);
                }

                var user = new User
                {
                    Username = model.Username,
                    Password = model.Password,
                    // Tạm gán role 1 ký tự cho phù hợp schema hiện tại
                    UserRole = "C"
                };
                db.Users.Add(user);

                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    CustomerPhone = model.CustomerPhone,
                    CustomerAddress = model.CustomerAddress,
                    Username = model.Username,
                    // Gán mật khẩu cho Customer nếu entity yêu cầu
                    CustomerPassword = model.Password
                };
                db.Customers.Add(customer);

                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    foreach (var eve in ex.EntityValidationErrors)
                    {
                        var entity = eve.Entry.Entity.GetType().Name;
                        foreach (var ve in eve.ValidationErrors)
                        {
                            ModelState.AddModelError("", $"{entity}.{ve.PropertyName}: {ve.ErrorMessage}");
                        }
                    }
                    return View(model);
                }
            }
            return View(model);
        }
        // 2. ĐĂNG NHẬP (GET)
        public ActionResult Login()
        {
            return View();
        }
        // 2. ĐĂNG NHẬP (POST)
        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.SingleOrDefault(u => u.Username == model.Username
                    && u.Password == model.Password
                    && (u.UserRole == "Customer" || u.UserRole == "C"));

                if (user != null)
                {
                    // Lưu trạng thái đăng nhập vào session
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;

                    // Lưu thông tin xác thực người dùng vào cookie
                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                }
            }
            return View(model);
        }
        // 3. ĐĂNG XUẤT
        public ActionResult Logout()
        {
            Session["Customer"] = null;
            Session["CustomerName"] = null;
            Session["CustomerID"] = null;
            return RedirectToAction("Index", "Home");
        }
        // GET: Account/ProfileInfo (Hiển thị trang thông tin tài khoản)
        // GET: Account/ProfileInfo (Hiển thị trang thông tin cá nhân)
        public ActionResult ProfileInfo()
        {
            // Lấy username từ session hay identity (FormsAuthentication)
            var username = Session["Username"] as string ?? (Request.IsAuthenticated ? User.Identity.Name : null);
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var curCustomer = db.Customers.SingleOrDefault(c => c.Username == username);
            if (curCustomer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Cập nhật session để dùng ở các chỗ khác
            Session["Customer"] = curCustomer;
            Session["CustomerName"] = curCustomer.CustomerName;
            Session["CustomerID"] = curCustomer.CustomerID;

            return View(curCustomer);
        }

        // POST: Account/UpdateProfile (Cập nhật SĐT, Email, Địa chỉ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(Customer model)
        {
            var customer = db.Customers.Find(model.CustomerID);
            if (customer != null)
            {
                customer.CustomerName = model.CustomerName;
                customer.CustomerPhone = model.CustomerPhone;
                customer.CustomerAddress = model.CustomerAddress;

                db.SaveChanges();

                Session["Customer"] = customer;
                Session["CustomerName"] = customer.CustomerName;
                ViewBag.Message = "Cập nhật thông tin thành công!";
            }
            return View("ProfileInfo", customer);
        }

        // POST: Account/ChangePassword (Đổi mật khẩu)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string oldPassword, string newPassword)
        {
            var customer = Session["Customer"] as Customer;
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var curCustomer = db.Customers.Find(customer.CustomerID);
            if (curCustomer != null && curCustomer.CustomerPassword == oldPassword)
            {
                curCustomer.CustomerPassword = newPassword;

                // Đồng bộ mật khẩu sang bảng User
                var user = db.Users.Find(curCustomer.Username);
                if (user != null)
                {
                    user.Password = newPassword;
                }

                db.SaveChanges();
                ViewBag.Message = "Đổi mật khẩu thành công!";
            }
            else
            {
                ViewBag.Error = "Mật khẩu cũ không chính xác!";
            }
            return View("ProfileInfo", curCustomer);
        }
    }
}