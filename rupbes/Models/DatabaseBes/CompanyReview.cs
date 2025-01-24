using System;
using System.ComponentModel.DataAnnotations;

namespace rupbes.Models.DatabaseBes
{
    public partial class CompanyReview
    {
        [Key]
        public int Id { get; set; }

        public int IdCompany { get; set; }        

        public DateTime DateReview { get; set; }

        public bool Confirm { get; set; }

        public string ReviewText { get; set; }


        public virtual Company Company { get; set; }  
    }
}