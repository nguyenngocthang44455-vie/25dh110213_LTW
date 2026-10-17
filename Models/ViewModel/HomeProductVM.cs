using System.Collections.Generic;
using PagedList;

namespace _25dh110213_LTW.Models.ViewModel
{
    public class HomeProductVM
    {
        public string SearchTerm { get; set; }

        // 10 sản phẩm bán chạy nhất (slider)
        public List<Product> FeaturedProducts { get; set; }

        // 20 sản phẩm mới nhất, phân trang 6 sản phẩm/trang
        public IPagedList<Product> NewProducts { get; set; }
    }
}
