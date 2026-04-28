namespace rupbes.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("rupbesby_admin.Engine")]
    public partial class Engine
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Engine()
        {
            Imgs = new HashSet<Imgs>();
        }

        public int Id { get; set; }

        [Required]
        [Display(Name = "Описание")]
        public string Name { get; set; }
                
        [Display(Name = "Отзыв")]
        public bool IsReview { get; set; }

        [Display(Name = "Аттестат")]
        public bool IsCertificate { get; set; }

        [Display(Name = "Партнер")]
        public bool IsPartner { get; set; }        

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Imgs> Imgs { get; set; }
    }
}
