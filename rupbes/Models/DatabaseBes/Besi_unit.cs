using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace rupbes.Models.DatabaseBes
{
    public class Besi_unit
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Besi_unit()
        {
            Besi_Price_Lists = new HashSet<Besi_price_list>();
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Besi_price_list> Besi_Price_Lists { get; set; }
    }
}
