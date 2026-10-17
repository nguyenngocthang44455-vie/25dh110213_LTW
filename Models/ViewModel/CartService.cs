using System.Web;

namespace _25dh110213_LTW.Models.ViewModel
{
    public class CartService
    {
        private readonly HttpSessionStateBase session;

        public CartService(HttpSessionStateBase session)
        {
            this.session = session;
        }

        public Cart GetCart()
        {
            var cart = session["Cart"] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                session["Cart"] = cart;
            }
            return cart;
        }

        public void ClearCart()
        {
            session["Cart"] = null;
        }
    }
}