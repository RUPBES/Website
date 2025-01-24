using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rupbes.Models.Products
{
    public partial class VersionProduct
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public bool isSale { get; set; }
        public string note { get; set; }
    }
}
