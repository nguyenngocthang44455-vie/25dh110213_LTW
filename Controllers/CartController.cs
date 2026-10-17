using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using _25dh110213_LTW.Models;
using _25dh110213_LTW.Models.ViewModel;
using PagedList;

namespace _25dh110213_LTW.Controllers
{
    public class CartController : Controller
    {
        private My_StoreEntities db = new My_StoreEntities();

        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // GET: Cart/Index (Gồm nhóm theo danh mục và sản phẩm tương tự)
        public ActionResult Index(int? page)
        {
            var cart = GetCartService().GetCart();
            var products = db.Products.ToList();
            var similarProducts = new List<Product>();

            if (cart.Items != null && cart.Items.Any())
            {
                // Lấy các sản phẩm cùng danh mục nhưng chưa có trong giỏ hàng
                similarProducts = products.Where(p => cart.Items.Any(ci => ci.Category == p.Category.CategoryName)
                    && !cart.Items.Any(ci => ci.ProductID == p.ProductID)).ToList();
            }

            int pageNumber = page ?? 1;
            int pageSize = cart.PageSize;

            // Gán danh sách sản phẩm tương tự đã phân trang vào Cart Model
            cart.SimilarProducts = similarProducts.OrderBy(p => p.ProductID).ToPagedList(pageNumber, pageSize);

            return View(cart);
        }

        public ActionResult AddToCart(int id, int quantity = 1)
        {
            var product = db.Products.Find(id);
            if (product != null)
            {
                var cartService = GetCartService();
                cartService.GetCart().AddItem(
                    product.ProductID,
                    product.ProductImage,
                    product.ProductName,
                    product.ProductPrice,
                    quantity,
                    product.Category != null ? product.Category.CategoryName : "Khác"
                );
            }
            return RedirectToAction("Index");
        }

        public ActionResult RemoveFromCart(int id)
        {
            var cartService = GetCartService();
            cartService.GetCart().RemoveItem(id);
            return RedirectToAction("Index");
        }

        public ActionResult ClearCart()
        {
            GetCartService().ClearCart();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult UpdateQuantity(int id, int quantity)
        {
            var cartService = GetCartService();
            cartService.GetCart().UpdateQuantity(id, quantity);
            return RedirectToAction("Index");
        }
    }
}