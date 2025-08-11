using DoAnTeam12.DAL;
using DoAnTeam12.Models.Employee;
using System;
using System.Linq;
using System.Web.Mvc;

namespace DoAnTeam12.Controllers.Employee
{
    public class EmployeeController : Controller
    {
        private EmployeeDAL dal = new EmployeeDAL();
        private EmployeeService employeeService = new EmployeeService();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List(string searchString)
        {
            var employees = dal.GetAllEmployees(null, null, searchString);
            ViewBag.SearchTerm = searchString;
            return View(employees);
        }

        [HttpPost]
        public JsonResult AjaxDelete(int id)
        {
            try
            {
                dal.DeleteNhanVien(id);
                return Json(new { success = true, message = "Employee deleted successfully." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in EmployeeController.AjaxDelete: {ex.Message}");
                return Json(new { success = false, message = "Error deleting employee: " + ex.Message });
            }
        }

        public ActionResult Add()
        {
            PopulateDropdowns();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(EmployeeModel model)
        {
            PopulateDropdowns();

            if (ModelState.IsValid)
            {
                try
                {
                    employeeService.AddAndWelcomeEmployee(model);
                    TempData["SuccessMessage"] = "Employee added successfully!";
                    return RedirectToAction("List");
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Email already exists!"))
                    {
                        ModelState.AddModelError("Email", "Email already exists.");
                    }
                    else if (ex.Message.Contains("Phone number already exists!"))
                    {
                        ModelState.AddModelError("PhoneNumber", "Phone number already exists.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "An error occurred while adding the employee: " + ex.Message);
                    }
                }
            }
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            var employee = dal.GetAllEmployees(null, null).FirstOrDefault(e => e.EmployeeID == id);
            if (employee == null)
            {
                return HttpNotFound();
            }

            PopulateDropdowns(employee.DepartmentID, employee.PositionID);
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EmployeeModel employee)
        {
            PopulateDropdowns(employee.DepartmentID, employee.PositionID);

            if (ModelState.IsValid)
            {
                try
                {
                    dal.UpdateNhanVien(employee);
                    TempData["SuccessMessage"] = "Employee updated successfully!";
                    return RedirectToAction("List");
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Email already exists!"))
                    {
                        ModelState.AddModelError("Email", "Email already exists.");
                    }
                    else if (ex.Message.Contains("Phone number already exists!"))
                    {
                        ModelState.AddModelError("PhoneNumber", "Phone number already exists.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "An error occurred while updating the employee: " + ex.Message);
                    }
                }
            }
            return View(employee);
        }

        public ActionResult Delete(int id)
        {
            var employee = dal.GetAllEmployees(null, null).FirstOrDefault(e => e.EmployeeID == id);
            if (employee == null)
            {
                return HttpNotFound();
            }
            return View(employee);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                dal.DeleteNhanVien(id);
                TempData["SuccessMessage"] = "Employee deleted successfully!";
                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                var employee = dal.GetAllEmployees(null, null).FirstOrDefault(e => e.EmployeeID == id);
                ModelState.AddModelError("", "An error occurred while deleting the employee: " + ex.Message);
                return View("Delete", employee);
            }
        }

        private void PopulateDropdowns(int? selectedDepartmentId = null, int? selectedPositionId = null)
        {
            var departments = dal.GetAllDepartments();
            ViewBag.Departments = new SelectList(departments, "DepartmentID", "DepartmentName", selectedDepartmentId);

            var positions = dal.GetAllPositions();
            ViewBag.Positions = new SelectList(positions, "PositionID", "PositionName", selectedPositionId);
        }
    }
}