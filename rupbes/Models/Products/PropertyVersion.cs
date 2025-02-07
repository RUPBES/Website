using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rupbes.Models.Products
{
    public class PropertyVersion
    {       
        public int PropertyId { get; set; }
        [ForeignKey("PropertyId")]
        public virtual Property Property { get; set; }
        public int VersionId { get; set; }
        [ForeignKey("VersionId")]
        public virtual VersionProduct VersionProduct { get; set; }
        public string value { get; set; }
    }
}
