namespace rupbes.Models.DatabaseBes
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

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

        public int DepartmentId { get; set; }  

        public bool is_work { get; set; }

        public bool is_email { get; set; }

        public bool is_phone { get; set; }

        public bool is_confirm_email { get; set; }

        public virtual Department Department { get; set; }

        public virtual Post Post { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Contact> Contacts { get; set; }
    }

    public class EmployeeCompareOrg : IComparer<Employee>
    {
        public int Compare(Employee p1, Employee p2)
        {
            int fl;
            fl = string.Compare(p1.last_name, p2.last_name);
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
    }
}
