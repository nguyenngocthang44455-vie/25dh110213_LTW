using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PayPal.Api;
using _25dh110213_LTW.Models;
using _25dh110213_LTW.Models.ViewModel;

namespace _25dh110213_LTW.Controllers
{
    public class PaypalController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        private APIContext CreateAPIContext(string accessToken)
        {
            var config = ConfigManager.Instance.GetProperties();
            var apiContext = new APIContext(accessToken)
            {
                Config = config
            };
            return apiContext;
        }

        private string GetAccessToken()
        {
            string clientId = System.Configuration.ConfigurationManager.AppSettings["PayPalClientId"];
            string clientSecret = System.Configuration.ConfigurationManager.AppSettings["PayPalClientSecret"];
            string mode = System.Configuration.ConfigurationManager.AppSettings["PayPalMode"];

            // Lấy token xác thực từ PayPal API
            string accessToken = new OAuthTokenCredential(clientId, clientSecret, new Dictionary<string, string> { { "mode", mode } }).GetAccessToken();
            return accessToken;
        }

        public ActionResult PaymentWithPaypal()
        {
            APIContext apiContext = CreateAPIContext(GetAccessToken());
            try
            {
                string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Paypal/PaymentWithPaypalExecuted?";
                var guid = Convert.ToString(new Random().Next(1000000));
                var createdPayment = CreatePayment(apiContext, baseURI + "guid=" + guid);

                var links = createdPayment.links.GetEnumerator();
                string paypalRedirectUrl = null;
                while (links.MoveNext())
                {
                    var lnk = links.Current;
                    if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                    {
                        paypalRedirectUrl = lnk.href;
                    }
                }
                Session.Add(guid, createdPayment.id);
                return Redirect(paypalRedirectUrl);
            }
            catch (Exception)
            {
                return View("FailureView");
            }
        }

        private Payment PaymentExecute(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            this.payment = new Payment() { id = paymentId };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        private Payment payment;
        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            var cart = GetCartService().GetCart();
            var itemList = new ItemList() { items = new List<Item>() };

            foreach (var item in cart.Items)
            {
                itemList.items.Add(new Item()
                {
                    name = item.ProductName,
                    currency = "USD", // PayPal sandbox thường yêu cầu USD hoặc quy đổi tương đương
                    price = (item.UnitPrice / 23000).ToString("F2"), // Ví dụ quy đổi VNĐ sang USD tạm thời
                    quantity = item.Quantity.ToString(),
                    sku = item.ProductID.ToString()
                });
            }

            var papayalPaymentMethod = new Payer() { payment_method = "paypal" };
            var redirUrl = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&cancel=true",
                return_url = redirectUrl
            };

            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = (cart.TotalValue() / 23000).ToString("F2")
            };

            var amount = new Amount()
            {
                currency = "USD",
                total = (cart.TotalValue() / 23000).ToString("F2"),
                details = details
            };

            var transactionList = new List<Transaction>();
            transactionList.Add(new Transaction()
            {
                description = "Transaction description",
                invoice_number = Convert.ToString(new Random().Next(1000000)),
                amount = amount,
                item_list = itemList
            });

            this.payment = new Payment()
            {
                intent = "sale",
                payer = papayalPaymentMethod,
                transactions = transactionList,
                redirect_urls = redirUrl
            };

            return this.payment.Create(apiContext);
        }

        public ActionResult PaymentWithPaypalExecuted(string guid, string PayerID)
        {
            APIContext apiContext = CreateAPIContext(GetAccessToken());
            try
            {
                var paymentId = Session[guid] as string;
                var executedPayment = PaymentExecute(apiContext, PayerID, paymentId);
                if (executedPayment.state.ToLower().Trim().Equals("approved"))
                {
                    // Thanh toán thành công, tiến hành lưu đơn hàng vào CSDL tương tự OrderController
                    var cart = GetCartService().GetCart();
                    var user = db.Users.SingleOrDefault(u => u.Username == User.Identity.Name);
                    var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);

                    var order = new Order
                    {
                        CustomerID = customer.CustomerID,
                        OrderDate = DateTime.Now,
                        TotalAmount = cart.TotalValue(),
                        PaymentStatus = "Đã thanh toán (PayPal)",
                        PaymentMethod = "Paypal",
                        DeliveryMethod = "Giao tiêu chuẩn",
                        ShippingAddress = customer.CustomerAddress
                    };
                    db.Orders.Add(order);

                    foreach (var item in cart.Items)
                    {
                        order.OrderDetails.Add(new OrderDetail
                        {
                            ProductID = item.ProductID,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        });
                    }
                    db.SaveChanges();
                    GetCartService().ClearCart();

                    return RedirectToAction("OrderSuccess", "Order", new { id = order.OrderID });
                }
            }
            catch (Exception)
            {
                return View("FailureView");
            }
            return RedirectToAction("Index", "Home");
        }
    }
}