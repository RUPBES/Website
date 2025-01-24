using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace rupbes.Models.DatabaseBes
{
    public class Tenders
    {
        [Key]
        public int id { get; set; }

        public string icetrade_id { get; set; }

        public DateTime? date { get; set; }

        public string icetrade_link { get; set; }

        public string contacts { get; set; }

        public string description { get; set; }

        public DateTime? uploadDate { get; set; }
    }
}