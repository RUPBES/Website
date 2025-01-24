using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rupbes.Models.Products
{
    public class Product
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
        public string codeTNVD { get; set; }
        public string note { get; set; }
        public int SubGroupProductId { get; set; }
        public int UnitId { get; set; }
        [ForeignKey("SubGroupProductId")]
        public virtual SubGroupProduct SubGroupProduct { get; set; }
        [ForeignKey("UnitId")]
        public virtual Unit Unit { get; set; }
        public int DepartmentId { get; set; }        
        [ForeignKey("DepartmentId")]
        public virtual Departments Departments { get; set; }
    }
}
