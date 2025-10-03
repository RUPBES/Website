namespace rupbes.Models.DatabaseBes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class EmployeeCategory
    {
        [Key]
        public int Id { get; set; }

        public string name { get; set; }        
    }
}
