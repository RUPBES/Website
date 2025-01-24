namespace rupbes.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("rupbesby_admin.Objects")]
    public partial class Objects
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Objects()
        {
            Departments = new HashSet<Departments>();
            Imgs = new HashSet<Imgs>();
        }

        public int id { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "��������")]
        public string title_ru { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "�������� �� ����������� �����")]
        public string title_bel { get; set; }

        [Required]
        [StringLength(1000)]
        [Display(Name = "�������� �� ���������� �����")]
        public string title_eng { get; set; }

        [Column(TypeName = "text")]
        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "��������")]
        public string desc_ru { get; set; }

        [Column(TypeName = "text")]
        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "�������� �� ����������� �����")]
        public string desc_bel { get; set; }

        [Column(TypeName = "text")]
        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "�������� �� ���������� �����")]
        public string desc_eng { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Departments> Departments { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Imgs> Imgs { get; set; }
    }
}
