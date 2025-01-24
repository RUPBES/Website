using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace rupbes.Models.ViewModels.Review
{
    public class IndexModel
    {
        [Required(ErrorMessage = "Заполните поле!")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Заполните поле!")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "Введитe адрес электронной почты в формате 'login@main.com'")]
        public string CompanyEmail { get; set; }

        [Required(ErrorMessage = "Заполните поле!")]
        [DataType(DataType.MultilineText)]
        [StringLength(2500, ErrorMessage = "Длина строки не должна превышать 2500 символов")]
        public string ReviewText { get; set; }
    }
}