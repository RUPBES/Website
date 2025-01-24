using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace rupbes.Models.DatabaseBes
{
    public class Document
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Document()
        {
            Besi_Type_Products_Photo = new HashSet<Besi_type_product>();
            Besi_Type_Products_Drawing = new HashSet<Besi_type_product>();
        }

        [Key]
        public int Id { get; set; }

        public string NumberDocument { get; set; }
        public string Link { get; set; }
        public DateTime DateDocument { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Besi_type_product> Besi_Type_Products_Photo { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Besi_type_product> Besi_Type_Products_Drawing { get; set; }
    }
}