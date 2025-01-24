namespace rupbes.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("rupbesby_admin.Users")]
    public partial class Users
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Users()
        {
            Usage_report = new HashSet<Usage_report>();
        }

        public int id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Логин")]
        public string login { get; set; }

        [Required]
        [StringLength(50)]
        public string pass { get; set; }

        public int id_role { get; set; }

        public int id_dep { get; set; }

        public virtual Departments Departments { get; set; }

        public virtual Roles Roles { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Usage_report> Usage_report { get; set; }
    }
}
