using DoAnTeam12.DAL;
using DoAnTeam12.Models.Department;
using DoAnTeam12.Models.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnTeam12.Controllers.Department
{
    
    public class DepartmentController : Controller
    {
        private DepartmentDAL _departmentDAL = new DepartmentDAL();
        public ActionResult Index()
        {
            DepartmentDAL departmentDAL = new DepartmentDAL();
            List<PositionsModel> positions = departmentDAL.GetAllPosition();
            ViewBag.Positions = positions;
            return View();
        }
        public ActionResult List()
        {
            DepartmentDAL departmentDAL = new DepartmentDAL();
            List<DepartmentModel> departments = departmentDAL.GetAllDepartments();
            List<PositionsModel> positions = departmentDAL.GetAllPosition();
            ViewBag.Departments = departments;
            return View(departments);
        }
        public ActionResult Details(int id)
        {
            List<EmployeeModel> employeesInDepartment = _departmentDAL.GetEmployeesByDepartment(id);
            ViewBag.DepartmentId = id;
            return View(employeesInDepartment);
        }
        
    }
}