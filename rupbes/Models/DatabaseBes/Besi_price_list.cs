using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace rupbes.Models.DatabaseBes
{
    public class Besi_price_list
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Besi_price_list()
        {
            Besi_Prices = new HashSet<Besi_price>();
        }

        [Key]
        public int Id { get; set; }
		public int codeTNVED { get; set; }

		public string Number { get; set; }
        public string Name { get; set; }
        public string Volume { get; set; }
        public string Weight { get; set; }
        public int IdBesi_unit { get; set; }
        public int IdBesi_type_product { get; set; }

        public virtual Besi_unit Besi_unit { get; set; }
        public virtual Besi_type_product Besi_type_product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Besi_price> Besi_Prices { get; set; }
    }
}
