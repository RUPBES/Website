namespace rupbes.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("rupbesby_admin.Departments")]
    public partial class Departments
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Departments()
        {
            Bosses = new HashSet<Bosses>();
            Certificates = new HashSet<Certificates>();
            Deps_to_groups = new HashSet<Deps_to_groups>();
            Mechanisms = new HashSet<Mechanisms>();
            News = new HashSet<News>();
            Realty = new HashSet<Realty>();
            Sale = new HashSet<Sale>();
            Services = new HashSet<Services>();
            Users = new HashSet<Users>();
            Vacancies = new HashSet<Vacancies>();
            Objects = new HashSet<Objects>();
        }

        public int id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Название")]
        public string name_ru { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Название на беларусском языке")]
        public string name_bel { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Название на английском языке")]
        public string name_eng { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Адрес")]
        public string adress_ru { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Адрес на беларусском языке")]
        public string adress_bel { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Адрес на английском языке")]
        public string adress_eng { get; set; }

        [StringLength(50)]
        [Display(Name = "Ссылка на сайт")]
        public string link { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Описание")]
        public string desc_ru { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Описание на беларусском языке")]
        public string desc_bel { get; set; }
        
        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Описание на английском языке")]
        public string desc_eng { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Текст на главную страницу")]
        public string main_text_ru { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Текст на главную страницу на беларусском языке")]
        public string main_text_bel { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Текст на главную страницу на английском языке")]
        public string main_text_eng { get; set; }

        public int type_id { get; set; }

        public int? id_img { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Короткое название")]
        public string short_name_ru { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Короткое название на беларусском языке")]
        public string short_name_bel { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Короткое название на английском языке")]
        public string short_name_eng { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Bosses> Bosses { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Certificates> Certificates { get; set; }

        public virtual Dep_types Dep_types { get; set; }

        public virtual Imgs Imgs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Deps_to_groups> Deps_to_groups { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Mechanisms> Mechanisms { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<News> News { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Realty> Realty { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Sale> Sale { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Services> Services { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Users> Users { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Vacancies> Vacancies { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Objects> Objects { get; set; }
    }
}
