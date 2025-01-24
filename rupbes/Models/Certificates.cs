namespace rupbes.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("rupbesby_admin.Certificates")]
    public partial class Certificates
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Certificates()
        {
            Imgs = new HashSet<Imgs>();
        }

        public int id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Название")]
        public string name { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Название на беларусском языке")]
        public string name_bel { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Название на английском языке")]
        public string name_eng { get; set; }

        public int id_dep { get; set; }

        public virtual Departments Departments { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Imgs> Imgs { get; set; }
    }
}
