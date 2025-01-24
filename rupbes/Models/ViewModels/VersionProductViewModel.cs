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
    public class VersionProductViewModel
    {
        public int id { get; set; }
        public int ProductId { get; set; }
        public string name { get; set; }           
        public bool isSale { get; set; }
        public string note { get; set; }
        public List<PropertyViewModel> properties { get; set; }
        public List<Imgs> Imgs { get; set; }
    }
}
