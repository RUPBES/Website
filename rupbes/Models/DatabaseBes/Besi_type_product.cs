using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace rupbes.Models.DatabaseBes
{
    public class Besi_type_product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Besi_type_product()
        {
            Besi_Price_Lists = new HashSet<Besi_price_list>();
        }

        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int IdPhoto { get; set; }
        public virtual Document Photo { get; set; }

        public int IdDrawing { get; set; }
        public virtual Document Drawing { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Besi_price_list> Besi_Price_Lists { get; set; }
    }
}
