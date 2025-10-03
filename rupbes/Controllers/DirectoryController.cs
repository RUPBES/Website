using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using rupbes.Models.DatabaseBes;

namespace rupbes.Controllers
{
    [Filters.Culture]
    public class DirectoryController : Controller
    {
        private Database db = new Database();

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Companies = db.Companies.Where(x => (x.Id >= 9) && (x.Id <= 17)).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult SearchName(string searchStr, bool showWorkers = false)
        {
            List<Employee> employeesall = new List<Employee>();
            if (!showWorkers)
            {
                employeesall = db.Employees.Where(x => x.is_work &&
                                    (x.EmployeeCategory == null ||
                                     x.EmployeeCategory.name == "Руководители" ||
                                     x.EmployeeCategory.name == "Специалисты")).ToList();
            }
            else
            {
                employeesall = db.Employees.Where(x => x.is_work).ToList();
            }

            List<Employee> employees = new List<Employee>();

            searchStr = searchStr.ToLower();

            foreach (Employee employee in employeesall)
            {
                var e = string.Concat(
                employee.last_name, " ", employee.first_name, " ", employee.father_name, " ",
                employee.first_name, " ", employee.last_name, " ", employee.father_name, " ",
                employee.last_name, " ", employee.father_name, " ", employee.first_name, " ",
                employee.first_name, " ", employee.father_name, " ", employee.last_name, " ",
                employee.father_name, " ", employee.last_name, " ", employee.first_name, " ",
                employee.father_name, " ", employee.first_name, " ", employee.last_name).ToLower();
                if (e.Contains(searchStr))
                {
                    employees.Add(employee);
                }
            }

            if (employees.Count > 0)
            {
                return PartialView("_SearchName", employees);
            }
            else
            {
                string result = "По вашему запросу ничего не найдено";
                return PartialView("_SearchEmpty", result);
            }
        }

        [HttpPost]
        public ActionResult SelectCompany(int companyId)
        {
            ViewBag.Departments = db.Departments.Where(x => x.CompanId == companyId && x.is_visible).OrderBy(x => x.Name);
            Company company = db.Companies.Find(companyId);
            return PartialView("_SelectCompany", company);
        }
        [HttpPost]
        public ActionResult SelectDepartment(int depId, bool showWorkers = false)
        {
            List<Employee> employees = new List<Employee>();
            if (!showWorkers)
            {
                employees = db.Employees.Where(x => x.is_work && 
                                     x.DepartmentId == depId &&
                                     (x.EmployeeCategory == null ||
                                     x.EmployeeCategory.name == "Руководители" ||
                                     x.EmployeeCategory.name == "Специалисты")).ToList();
            }
            else
            {
                employees = db.Employees.Where(x => x.is_work && x.DepartmentId == depId).ToList();
            }
            
            employees.Sort(new EmployeeDepartmentCompareOrg());

            //сортировка в справочнике
            //string text = "начальник";
            //var depName = db.Departments.Where(x => x.Id == depId).Select(x => x.Name).FirstOrDefault().ToString();

            //if (depName.ToLower() == "руководство")
            //{
            //    for (int i = 0; i < employees.Count(); i++)
            //    {
            //        text = "директор";
            //        var text1 = "директора";
            //        bool index = employees[i].Post.Name.ToLower().Contains(text);
            //        bool index1 = employees[i].Post.Name.ToLower().Contains(text1);

            //        if (index && !index1)
            //        {
            //            var mainEmp = employees[i];
            //            employees[i] = employees[0];
            //            employees[0] = mainEmp;
            //            break;
            //        }
            //    }
            //}
            //else
            //if (depName.ToLower() == "отдел главного механика")
            //{

            //    text = "главный механик";
            //    int index = -1;

            //    for (int i = 0; i < employees.Count(); i++)
            //    {
            //        if (employees[i].Post.Name.ToLower().Contains(text))
            //        {
            //            index = i;
            //            break;
            //        }
            //    }
            //    if (0 != index && index != -1)
            //    {
            //        var mainEmp = employees[index];
            //        employees[index] = employees[0];
            //        employees[0] = mainEmp;
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < employees.Count(); i++)
            //    {
            //        var text1 = "начальника";
            //        bool index = employees[i].Post.Name.ToLower().Contains(text);
            //        bool index1 = employees[i].Post.Name.ToLower().Contains(text1);
            //        if (index && !index1)
            //        {
            //            var mainEmp = employees[i];
            //            employees[i] = employees[0];
            //            employees[0] = mainEmp;
            //            break;
            //        }
            //    }
            //}

            ViewBag.Department = db.Departments.Find(depId);
            return PartialView("_SelectDepartment", employees);
        }
    }
}