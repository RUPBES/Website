namespace rupbes.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("rupbesby_admin.Vacancies")]
    public partial class Vacancies
    {
        public int id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "��������")]
        public string vacancy_ru { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "�������� �� ����������� �����")]
        public string vacancy_bel { get; set; }

        [StringLength(50)]
        [Display(Name = "���������� �����")]
        public string payment { get; set; }

        public int id_dep { get; set; }

        [Display(Name = "����������")]
        public string requirement_ru { get; set; }

        [Display(Name = "���������� �� ����������� �����")]
        public string requirement_bel { get; set; }

        public virtual Departments Departments { get; set; }

    }
}
