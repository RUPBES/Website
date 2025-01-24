using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rupbes.Models.Products
{
    public class Property
    {
        [Key]
        public int id { get; set; }
        public string name { get; set; }
    }
}
