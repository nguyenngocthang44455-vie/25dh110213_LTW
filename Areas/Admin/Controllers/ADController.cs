using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using _25dh110213_LTW.Models;

namespace _25dh110213_LTW.Areas.Admin.Controllers
{
    public class ADController : Controller
    {
        // GET: Admin/AD
        public ActionResult Index()
        {
            using (var db = new My_StoreEntities())
            {
                var rows = db.Products.GroupBy(p => p.Category.CategoryName)
                                      .Select(g => new { Name = g.Key, Count = g.Count() }).ToList();
                var list = new List<object[]> { new object[] { "Danh mục", "Số sản phẩm" } };
                list.AddRange(rows.Select(r => new object[] { r.Name, r.Count }));
                ViewBag.ChartData = JsonConvert.SerializeObject(list);
            }
            return View();
        }
    }
}
