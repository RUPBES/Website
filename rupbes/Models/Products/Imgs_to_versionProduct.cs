using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rupbes.Models.Products
{
    public class Imgs_to_versionProduct
    {
        public int ImgsId { get; set; }
        [ForeignKey("ImgsId")]
        public virtual Imgs Imgs { get; set; }
        public int VersionProductId { get; set; }
        [ForeignKey("VersionProductId")]
        public virtual VersionProduct VersionProduct { get; set; }
    }
}
