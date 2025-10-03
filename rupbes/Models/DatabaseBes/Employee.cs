namespace rupbes.Models.DatabaseBes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;
    using System.Linq;

    public partial class Employee
    {
        public Employee()
        {
            Contacts = new HashSet<Contact>();
        }

        [Key]
        public int Id { get; set; }

        public string first_name { get; set; }
        [Display(Name = "Фамилия")]
        public string last_name { get; set; }
        [Display(Name = "Отчество")]
        public string father_name { get; set; }

        public int PostId { get; set; }
        public int? CategoryId { get; set; }

        public int DepartmentId { get; set; }  

        public bool is_work { get; set; }

        public bool is_email { get; set; }

        public bool is_phone { get; set; }

        [NotMapped]
        public bool? is_on_vacation
        {
            get
            {
                // Логика проверки отпуска
                if (Vacations == null) return false;

                var now = DateTime.Now;
                return Vacations.Any(v => v.date_begin <= now && v.date_end >= now);
            }
        }

        public bool is_confirm_email { get; set; }

        public virtual Department Department { get; set; }
        public virtual ICollection<Vacation> Vacations { get; set; }

        public virtual Post Post { get; set; }
        public virtual EmployeeCategory EmployeeCategory { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Contact> Contacts { get; set; }
    }

    public class EmployeeDepartmentCompareOrg : IComparer<Employee>
    {
        public int Compare(Employee p1, Employee p2)
        {
            // Сначала сортируем по категориям: Руководители -> Специалисты -> Рабочие
            int categoryCompare = GetCategoryPriority(p1).CompareTo(GetCategoryPriority(p2));
            if (categoryCompare != 0)
                return categoryCompare;

            // Если обе категории "Руководители", сортируем по приоритету должности
            if (GetCategoryPriority(p1) == 0 && GetCategoryPriority(p2) == 0)
            {
                int positionPriorityCompare = GetPositionPriority(p1).CompareTo(GetPositionPriority(p2));
                if (positionPriorityCompare != 0)
                    return positionPriorityCompare;
            }

            // Затем сортируем по ФИО
            int fl = string.Compare(p1.last_name, p2.last_name);
            if (fl == 0)
            {
                fl = string.Compare(p1.first_name, p2.first_name);
                if (fl == 0)
                {
                    return string.Compare(p1.father_name, p2.father_name);
                }
                else return fl;
            }
            else return fl;
        }

        private int GetCategoryPriority(Employee employee)
        {
            if (employee?.EmployeeCategory?.name == "Руководители")
                return 0;
            else if (employee?.EmployeeCategory?.name == "Специалисты")
                return 1;
            else if (employee?.EmployeeCategory?.name == "Рабочие")
                return 2;
            else
                return 3; // Для остальных категорий
        }

        private int GetPositionPriority(Employee employee)
        {
            var positionName = employee?.Post?.Name?.ToLower() ?? "";

            // Исключаем все должности с "заместитель", "зам" в начале
            if ((positionName.StartsWith("заместитель") || positionName.StartsWith("зам.")) && positionName.Contains("начальника"))
                return 5; // Все заместители в конце

            // Главные должности (только те, что не являются заместителями)
            if (positionName.Contains("генеральный директор"))
                return 0;
            else if (positionName.Contains("директор") && !positionName.Contains("заместитель"))
                return 1;
            else if (positionName.Contains("начальник"))
                return 2;
            else if (positionName.Contains("главный инженер"))
                return 3;
            else if (positionName.Contains("главный механик") || positionName.Contains("главный бухгалтер"))
                return 4;
            else
                return 5; // Остальные руководители
        }
    }
}
