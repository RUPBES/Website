namespace rupbes.Models.DatabaseBes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public class Vacation
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }        

        public DateTime date_begin { get; set; }
        public DateTime date_end { get; set; }
        public virtual Employee Employee { get; set; }
    }
}
