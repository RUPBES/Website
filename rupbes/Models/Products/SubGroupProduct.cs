using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rupbes.Models.Products
{
    public class SubGroupProduct
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public int GroupProductId { get; set; }
        [ForeignKey("GroupProductId")]
        public virtual GroupProduct GroupProduct { get; set; }
    }
}
