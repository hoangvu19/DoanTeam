using DoAnTeam12.DAL;
using DoAnTeam12.Models.Payroll;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System;
using DoAnTeam12.Services;

namespace DoAnTeam12.Controllers
{
    public class PayrollController : Controller
    {
        private readonly PayrollDAL _payrollDal;
        private readonly PayrollService _payrollService; 

        public PayrollController()
        {
            _payrollDal = new PayrollDAL();
            _payrollService = new PayrollService();
        }

        private void LoadDropdowns(int? selectedDeptId = null, int? selectedPosId = null)
        {
            var departments = _payrollDal.GetDepartments();
            ViewBag.Departments = new SelectList(departments, "DepartmentID", "DepartmentName", selectedDeptId);

            var positions = _payrollDal.GetPositions();
            ViewBag.Positions = new SelectList(positions, "PositionID", "PositionName", selectedPosId);
        }

        public ActionResult Index(int? departmentId, int? positionId, DateTime? salaryMonth, string search)
        {
            LoadDropdowns(departmentId, positionId);

            ViewBag.SalaryMonth = salaryMonth;
            ViewBag.Search = search;

            var payrollList = _payrollDal.GetPayrollsList(search); 

            if (departmentId.HasValue)
                payrollList = payrollList.Where(p => p.DepartmentID == departmentId.Value).ToList();

            if (positionId.HasValue)
                payrollList = payrollList.Where(p => p.PositionID == positionId.Value).ToList();

            if (salaryMonth.HasValue)
                payrollList = payrollList.Where(p => p.SalaryMonth.Year == salaryMonth.Value.Year
                    && p.SalaryMonth.Month == salaryMonth.Value.Month).ToList();

            payrollList = payrollList.OrderByDescending(p => p.SalaryMonth).ToList();

            return View(payrollList);
        }


        public ActionResult Details(int id)
        {
            var payroll = _payrollDal.GetPayrollDetails(id);
            if (payroll == null)
                return HttpNotFound();

            return View(payroll);
        }

        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(PayrollModels payroll)
        {
            if (!ModelState.IsValid)
                return View(payroll);

            if (payroll.SalaryMonth > DateTime.Now)
            {
                ViewBag.Error = "Ngày tháng bảng lương không được lớn hơn ngày hiện tại.";
                return View(payroll);
            }

            if (payroll.WorkDays < 0 || payroll.AbsentDays < 0 || payroll.LeaveDays < 0)
            {
                ViewBag.Error = "Số ngày làm việc, nghỉ phép và nghỉ không có lý do không được âm.";
                return View(payroll);
            }

            int totalDays = payroll.WorkDays + payroll.AbsentDays + payroll.LeaveDays;
            if (totalDays > 31)
            {
                ViewBag.Error = "Tổng số ngày làm việc, nghỉ phép và nghỉ không có lý do không được lớn hơn 31 ngày.";
                return View(payroll);
            }

            if (totalDays != 30)
            {
                ViewBag.Error = "Tổng số ngày làm việc, nghỉ phép và nghỉ không có lý do phải bằng 30 ngày.";
                return View(payroll);
            }

            if (!_payrollDal.EmployeeExists(payroll.EmployeeID))
            {
                ViewBag.Error = "Nhân viên không tồn tại.";
                return View(payroll);
            }

            if (_payrollDal.PayrollExists(payroll.EmployeeID, payroll.SalaryMonth))
            {
                ViewBag.Error = "Bảng lương cho nhân viên và tháng này đã tồn tại.";
                return View(payroll);
            }

            if (_payrollService.AddPayrollAndNotify(payroll))
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error = "Thêm bảng lương thất bại.";
            return View(payroll);
        }
        public ActionResult Edit(int id)
        {
            var payroll = _payrollDal.GetPayrollBySalaryID(id); 
            if (payroll == null)
            {
                return HttpNotFound();
            }

            ViewBag.EmployeeName = _payrollDal.GetEmployeeNameById(payroll.EmployeeID);
            return View(payroll);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PayrollModels model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Dữ liệu không hợp lệ.";
                return View(model);
            }

            if (model.SalaryMonth > DateTime.Now)
            {
                ViewBag.Error = "Ngày tháng bảng lương không được lớn hơn ngày hiện tại.";
                return View(model);
            }

            if (model.WorkDays < 0 || model.AbsentDays < 0 || model.LeaveDays < 0)
            {
                ViewBag.Error = "Số ngày làm việc, nghỉ phép và nghỉ không có lý do không được âm.";
                return View(model);
            }

            int totalDays = model.WorkDays + model.AbsentDays + model.LeaveDays;
            if (totalDays != 30)
            {
                ViewBag.Error = "Tổng số ngày làm việc, nghỉ phép và nghỉ không có lý do phải bằng đúng 30 ngày.";
                return View(model);
            }

            var employeeName = _payrollDal.GetEmployeeNameById(model.EmployeeID);
            if (string.IsNullOrEmpty(employeeName))
            {
                ViewBag.Error = "Không tìm thấy nhân viên với mã " + model.EmployeeID;
                return View(model);
            }

            bool success = _payrollDal.UpdatePayroll(model);
            if (success)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Error = "Không thể cập nhật bảng lương.";
                return View(model);
            }
        }



        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                bool result = _payrollDal.DeletePayroll(id);
                if (result)
                    return Json(new { success = true, message = "Xóa thành công." });
                else
                    return Json(new { success = false, message = "Xóa thất bại." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetEmployeeName(int id)
        {
            try
            {
                var name = _payrollDal.GetEmployeeNameById(id);
                if (!string.IsNullOrEmpty(name))
                    return Json(new { success = true, name }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = "Không tìm thấy nhân viên." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi lấy tên nhân viên: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
