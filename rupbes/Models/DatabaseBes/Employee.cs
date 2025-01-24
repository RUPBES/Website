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

        public int PostId { get; set; }

        public int DepartmentId { get; set; }

        public int PersonId { get; set; }

        public string TabelNumber { get; set; }

        public bool is_work { get; set; }

        public bool is_email { get; set; }

        public bool is_phone { get; set; }
        public bool is_confirm_email { get; set; }

        public virtual Department Department { get; set; }

        public virtual Post Post { get; set; }

        public virtual Person Person { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Contact> Contacts { get; set; }
    }

    public class EmployeeCompareOrg : IComparer<Employee>
    {
        public int Compare(Employee p1, Employee p2)
        {
            int fl;
            fl = string.Compare(p1.Person.LastName, p2.Person.LastName);
            if (fl == 0)
            {
               fl = string.Compare(p1.Person.FirstName, p2.Person.FirstName);
               if (fl == 0)
               {
                  return string.Compare(p1.Person.FatherName, p2.Person.FatherName);
               }
               else return fl;
            }
            else return fl;            
        }
    }
}
