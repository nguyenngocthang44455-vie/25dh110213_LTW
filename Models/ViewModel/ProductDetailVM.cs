using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace _25dh110213_LTW.Models.ViewModel
{
    public class ProductDetailsVM
    {
        public Product product { get; set; }
        public int quantity { get; set; } = 1;
        public decimal estimatedValue => quantity * product.ProductPrice;

        // Các thuộc tính hỗ trợ phân trang
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 3; // Số sản phẩm mỗi trang

        // Danh sách 8 sản phẩm cùng danh mục
        public List<Product> RelatedProducts { get; set; }

        // Danh sách 8 sản phẩm bán chạy nhất cùng danh mục
        public IPagedList<Product> TopProducts { get; set; }
    }
    public class ProductDetailVM
    {
        public Product product { get; set; }
        public int quantity { get; set; } = 1;
        public decimal estimatedValue
        {
            get { return quantity * product.ProductPrice; }
        }

        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 3;

        public List<Product> RelatedProducts { get; set; }
        public IPagedList<Product> TopProducts { get; set; }
    }
}