namespace rupbes.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("rupbesby_admin.Realty")]
    public partial class Realty
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Realty()
        {
            Imgs = new HashSet<Imgs>();
        }

        public int id { get; set; }

        [Required]
        [Display(Name = "��������� �� ������� �����")]
        public string title { get; set; }

        [Required]
        [Display(Name = "��������� �� ����������� �����")]
        public string title_bel { get; set; }

        [DataType(DataType.MultilineText)]
        [Column(TypeName = "text")]
        [Display(Name = "��������")]
        [Required]
        public string desc { get; set; }

        [DataType(DataType.MultilineText)]
        [Column(TypeName = "text")]
        [Display(Name = "�������� �� ����������� �����")]
        [Required]
        public string desc_bel { get; set; }

        [Required]
        [Display(Name = "������ �� ������� �����")]
        public string adress { get; set; }

        [Required]
        [Display(Name = "������ �� ����������� �����")]
        public string adress_bel { get; set; }

        public int id_dep { get; set; }

        public virtual Departments Departments { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Imgs> Imgs { get; set; }
    }
}
