using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _25dh110213_LTW.Models;
using _25dh110213_LTW.Models.ViewModel;
using System.Data.Entity;

namespace _25dh110213_LTW.Controllers
{
    public class OrderController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // GET: Order/Checkout
        [Authorize]
        public ActionResult Checkout()
        {
            var cart = GetCartService().GetCart();
            if (cart.Items == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new CheckoutVM
            {
                CartItems = cart.Items.ToList(),
                TotalAmount = cart.TotalValue(),
                ShippingAddress = customer.CustomerAddress,
                CustomerID = customer.CustomerID,
                Username = customer.Username
            };

            return View(model);
        }

        // POST: Order/Checkout
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutVM model)
        {
            var cart = GetCartService().GetCart();
            if (cart.Items == null || !cart.Items.Any())
            {
                return RedirectToAction("Index", "Home");
            }

            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);

            if (ModelState.IsValid)
            {
                // Thiết lập trạng thái thanh toán dựa trên phương thức thanh toán
                string paymentStatus = "Chưa thanh toán";
                switch (model.PaymentMethod)
                {
                    case "Tiền mặt": paymentStatus = "Thanh toán tiền mặt"; break;
                    case "Paypal": paymentStatus = "Thanh toán paypal"; break;
                    case "Thẻ ngân hàng": paymentStatus = "Chưa thanh toán"; break;
                    default: paymentStatus = "Chưa thanh toán"; break;
                }

                // Tạo đối tượng Order mới
                var order = new Order
                {
                    CustomerID = customer.CustomerID,
                    OrderDate = DateTime.Now,
                    TotalAmount = cart.TotalValue(),
                    PaymentStatus = paymentStatus,
                    PaymentMethod = model.PaymentMethod,
                    DeliveryMethod = model.ShippingMethod,
                    ShippingAddress = model.ShippingAddress
                };

                db.Orders.Add(order);

                // Thêm chi tiết đơn hàng
                foreach (var item in cart.Items)
                {
                    var orderDetail = new OrderDetail
                    {
                        ProductID = item.ProductID,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    order.OrderDetails.Add(orderDetail);
                }

                db.SaveChanges();

                // Xóa giỏ hàng sau khi đặt thành công
                Session["Cart"] = null;

                return RedirectToAction("OrderSuccess", new { id = order.OrderID });
            }

            model.CartItems = cart.Items.ToList();
            model.TotalAmount = cart.TotalValue();
            return View(model);
        }

        // GET: Order/OrderSuccess/5
        public ActionResult OrderSuccess(int id)
        {
            var order = db.Orders.Include("OrderDetails").SingleOrDefault(o => o.OrderID == id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Order/MyOrder
        [Authorize]
        public ActionResult MyOrder()
        {
            var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
            var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);

            var orders = db.Orders.Where(o => o.CustomerID == customer.CustomerID).OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }
    }
}