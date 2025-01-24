using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rupbes.Models.Products;
using rupbes.Models;

namespace rupbes.Models.ViewModels
{
    public class ProductViewModel
    {   
        public int id { get; set; }
        public string name { get; set; }
        public string codeTNVD { get; set; }
        public string note { get; set; }
        public string groupProduct { get; set; }
        public string subGroupProduct { get; set; }
        public int UnitId { get; set; }
        public string unitName { get; set; }
        public int DepartmentId { get; set; }
        public string departmentName { get; set; }
        public List<PropertyViewModel> properties { get; set; }
        public List<Imgs> Imgs { get; set; }
    }
}
