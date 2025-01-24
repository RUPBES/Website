namespace rupbes.Models
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using System.Data.Entity.Spatial;

    [Table("rupbesby_admin.News")]
    public partial class News
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public News()
        {
            Imgs = new HashSet<Imgs>();
        }

        [HiddenInput(DisplayValue = false)]
        public int id { get; set; }

        [Required]
        [Display(Name = "Заголовок новости на русском языке")]
        public string title_ru { get; set; }

        [Required]
        [Display(Name = "Заголовок на белорусском языке")]
        public string title_bel { get; set; }

        [DataType(DataType.MultilineText)]
        [Column(TypeName = "text")]
        [Display(Name = "Текст новости")]
        [Required]
        public string body_ru { get; set; }

        [DataType(DataType.MultilineText)]
        [Column(TypeName = "text")]
        [Display(Name = "Текст новости на белорусском языке")]
        [Required]
        public string body_bel { get; set; }

        [DataType(DataType.Date)]
        public DateTime date { get; set; }

        [Display(Name = "Тип новости")]
        public int type_id { get; set; }

        [Display(Name = "Тип новости")]
        public virtual News_type News_type { get; set; }

        public int id_dep { get; set; }

        public virtual Departments Departments { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Imgs> Imgs { get; set; }
    }
}
