using DoAnTeam12.DAL;
using DoAnTeam12.Models.Attendance;
using DoAnTeam12.Services;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace DoAnTeam12.Controllers.Attendance
{
    public class NotificationController : Controller
    {
        private readonly NotificationService _notificationService;
        private readonly NotificationDAl _notificationDAL;

        public NotificationController()
        {
            _notificationService = new NotificationService();
            _notificationDAL = new NotificationDAl();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {
            try
            {
                var notifications = _notificationDAL.GetAllNotifications();
                return View(notifications ?? new List<NotificationModel>());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[NotificationController.List] Lỗi: {ex}");
                ViewBag.Message = "Đã xảy ra lỗi khi tải danh sách thông báo.";
                return View(new List<NotificationModel>());
            }
        }

        public ActionResult Alerts()
        {
            try
            {
                ViewBag.AnniversaryAlerts = _notificationService.GetAnniversaryAlerts() ?? new List<string>();
                ViewBag.SalaryVarianceAlerts = _notificationService.GetSalaryVarianceAlerts() ?? new List<string>();
                ViewBag.LeaveAlerts = _notificationService.GetLeaveAlerts() ?? new List<string>();

                return View();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[NotificationController.Alerts] Lỗi: {ex}");
                ViewBag.Message = $"Đã xảy ra lỗi khi xử lý cảnh báo: {ex.Message}";
                ViewBag.AnniversaryAlerts = new List<string>();
                ViewBag.SalaryVarianceAlerts = new List<string>();
                ViewBag.LeaveAlerts = new List<string>();
                return View();
            }
        }

        [HttpPost]
        public ActionResult SendPayrollEmails()
        {
            try
            {
                _notificationService.SendAlertsToEmployees();
                TempData["SuccessMessage"] = "Đã gửi thông báo email đến nhân viên thành công.";
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[NotificationController.SendPayrollEmails] Lỗi: {ex}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi gửi email thông báo.";
            }

            return RedirectToAction("Alerts"); 
        }

    }
}
