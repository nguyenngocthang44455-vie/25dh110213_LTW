using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

namespace _25dh110213_LTW.Models
{
    public partial class Product
    {
        [NotMapped]
        public HttpPostedFileBase UploadImg { get; set; }
    }
}