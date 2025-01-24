using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace rupbes.Models
{
    public class PersonMessage
    {
        [Required(ErrorMessage = "Заполните поле!")]
        [Display(Name = "Фамилия, собственное имя, отчество (если таковое имеется), либо инициалы гражданина * ")]
        public string Sender { get; set; }

        [Required(ErrorMessage = "Заполните поле!")]
        [Display(Name = "Наименование, адрес организации и должность лица, которым направляется обращение * ")]
        public string Reciver { get; set; }

        [Required(ErrorMessage = "Заполните поле!")]
        [Display(Name = "Тема Вашего электронного обращения * ")]
        public string Theme { get; set; }

        [Required(ErrorMessage = "Заполните поле!")]
        [Display(Name = "Ваш почтовый индекс и адрес места жительства (места пребывания) * ")]
        public string Adress { get; set; }

        [Required(ErrorMessage = "Заполните поле!")]
        [Display(Name = "Адрес электронной почты для ответа Вам * ")]
        [EmailAddress(ErrorMessage = "Введитe корректный адрес электронной почты")]
        public string Email {get; set;}

        [DataType(DataType.MultilineText)]
        [Display(Name = "Изложение сути обращения (до 2500 символов) * ")]
        [Required(ErrorMessage = "Заполните поле!")]
        [StringLength(2500, ErrorMessage = "Текст больше 2500 символов")]
        public string Text { get; set; }

    }
}