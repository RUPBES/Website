using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace rupbes.Models.DatabaseBes
{
    public class Besi_price
    {
        [Key]
        public int Id { get; set; }

        public string Price { get; set; }
        public string Price_w_nds { get; set; }
        
        public int IdBesi_price_list { get; set; }

        public DateTime DatePrice { get; set; }

        public virtual Besi_price_list Besi_price_list { get; set; }
    }
}
