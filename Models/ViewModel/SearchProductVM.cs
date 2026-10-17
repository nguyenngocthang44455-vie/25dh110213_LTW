using PagedList;

namespace _25dh110213_LTW.Models.ViewModel
{
    public class SearchProductVM
    {
        public string SearchTerm { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortOrder { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public IPagedList<Product> Products { get; set; }
    }
}
