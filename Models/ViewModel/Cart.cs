using System.Collections.Generic;
using System.Linq;
using _25dh110213_LTW.Models;
using PagedList;

namespace _25dh110213_LTW.Models.ViewModel
{
    public class Cart
    {
        private List<CartItem> items = new List<CartItem>();
        public IEnumerable<CartItem> Items => items;

        public void AddItem(int productId, string productImage, string productName, decimal unitPrice, int quantity, string category)
        {
            var existingItem = items.FirstOrDefault(i => i.ProductID == productId);
            if (existingItem == null)
            {
                items.Add(new CartItem { ProductID = productId, ProductImage = productImage, ProductName = productName, UnitPrice = unitPrice, Quantity = quantity, Category = category });
            }
            else
            {
                existingItem.Quantity += quantity;
            }
        }

        public void RemoveItem(int productId)
        {
            items.RemoveAll(i => i.ProductID == productId);
        }

        public decimal TotalValue()
        {
            return items.Sum(i => i.TotalPrice);
        }

        public void Clear()
        {
            items.Clear();
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var item = items.FirstOrDefault(i => i.ProductID == productId);
            if (item != null)
            {
                item.Quantity = quantity;
            }
        }

        // Nhóm sản phẩm trong giỏ theo danh mục
        public List<IGrouping<string, CartItem>> GroupedItems => items.GroupBy(i => i.Category).ToList();

        // Thuộc tính hỗ trợ phân trang sản phẩm tương tự
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 6;
        public PagedList.IPagedList<Product> SimilarProducts { get; set; }
    }
}