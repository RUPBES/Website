namespace rupbes.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("rupbesby_admin.Bosses")]
    public partial class Bosses
    {
        public int id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name ="Имя")]
        public string name { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Имя на беларусском языке")]
        public string name_bel { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Имя на английском языке")]
        public string name_eng { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Должность")]
        public string post { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Должность на беларусском языке")]
        public string post_bel { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Должность на английском языке")]
        public string post_eng { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Описание")]
        public string desc { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Описание на беларусском языке")]
        public string desc_bel { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Описание на английском языке")]
        public string desc_eng { get; set; }

        public int id_img { get; set; }

        public int id_dep { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "День приема")]
        public string meet_day { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "День приема на беларусском языке")]
        public string meet_day_bel { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "День приема на английском языке")]
        public string meet_day_eng { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Время приёма")]
        public string meet_time { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Телефон")]
        public string phone { get; set; }

        public virtual Departments Departments { get; set; }

        public virtual Imgs Imgs { get; set; }
    }
}
