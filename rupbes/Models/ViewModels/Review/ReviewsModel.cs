using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using rupbes.Models.DatabaseBes;

namespace rupbes.Models.ViewModels.Review
{
    public class ReviewsModel
    {
        public string DateReview { get; set; }        
        public Company Filial { get; set; }
        public string LinkToFilialSite { get; set; }
        public string ReviewText { get; set; }
    }
}