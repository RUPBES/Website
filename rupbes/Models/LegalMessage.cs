using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace rupbes.Models
{
    public class LegalMessage
    {
        //public static string Type { get; set; } = "person";
        [Required(ErrorMessage = "Заполните поле!")]
        public string Sender { get; set; }

        [Required(ErrorMessage = "Заполните поле!")]
        public string SenderName { get; set; }

        [Required(ErrorMessage = "Заполните поле!")]
        public string Receiver { get; set; }

        [Required(ErrorMessage = "Заполните поле!")]
        public string Theme { get; set; }

        [Required(ErrorMessage = "Заполните поле!")]
        public string Adress { get; set; }

        [Required(ErrorMessage = "Заполните поле!")]
        [EmailAddress(ErrorMessage = "Введитe корректный адрес электронной почты")]
        public string Email { get; set; }

        [DataType(DataType.MultilineText)]
        [Required(ErrorMessage = "Заполните поле!")]
        [StringLength(2500,ErrorMessage ="Текст больше 2500 символов")]
        public string Text { get; set; }

    }
}