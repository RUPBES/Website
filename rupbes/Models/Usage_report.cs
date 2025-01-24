namespace rupbes.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("rupbesby_admin.Usage_report")]
    public partial class Usage_report
    {
        public int id { get; set; }

        public int id_user { get; set; }

        [Required]
        [Display(Name = "Таблица")]
        public string table { get; set; }

        [Required]
        [Display(Name = "Заголовок")]
        public string title { get; set; }

        [Required]
        [Display(Name = "Действие")]
        public string action { get; set; }

        [Display(Name = "Дата")]
        public DateTime date { get; set; }

        public virtual Users Users { get; set; }
    }
}
