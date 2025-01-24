using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace rupbes.Models
{
    public class ResponseVacancy
    {        
        public string VacancyName { get; set; }     // Наименование вакансии
        public int VacancyId { get; set; }          // Идентификатор вакансии в БД

        [RegularExpression("[A-z- А-я]{2,20}", ErrorMessage ="Имя может содержать только русские или латинские буквы!")]
        [Required(ErrorMessage = "Заполните поле!")]
        [Display(Name = "Ваше имя * ")]
        public string FirstName { get; set; }       // Имя соискателя

        [RegularExpression("[A-z- А-я]{2,20}", ErrorMessage = "Фамилия может содержать только русские или латинские буквы!")]
        [Required(ErrorMessage = "Заполните поле!")]
        [Display(Name = "Ваша фамилия * ")]
        public string LastName { get; set; }        // Фамилия соискателя

        [DataType(DataType.EmailAddress)]        
        [Display(Name = "Ваш адрес электронной почты")]
        [EmailAddress(ErrorMessage = "Введитe корректный адрес электронной почты")]
        public string Email { get; set; }           // Email соискателя

        [DataType(DataType.MultilineText)]
        [Display(Name = "Сопроводительное письмо (до 2500 символов)")]
        [StringLength(2500, ErrorMessage = "Длина строки не должна превышать 2500 символов")]
       
        public string Text { get; set; }            // Сопроводителное письмо
    }
}