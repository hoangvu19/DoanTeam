using DoAnTeam12.DAL;
using DoAnTeam12.Models.Account;
using System;
using System.Web.Mvc;
using System.Data.SqlClient;

namespace DoAnTeam12.Controllers
{
    public class AccountController : Controller
    {
        private readonly AccountDAL _accountDAL = new AccountDAL();

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserModel model)
        {
           

            try
            {
                string userRole = _accountDAL.AuthenticateUser(model.UserName, model.Password);
                if (!string.IsNullOrEmpty(userRole))
                {
                    Session["UserName"] = model.UserName;
                    Session["IsLoggedIn"] = true;
                    Session["Role"] = userRole;

                    switch (userRole.ToLower())
                    {
                        case "admin":
                            return RedirectToAction("Dashboard", "Dashboard");
                        case "hr":
                            return RedirectToAction("List", "Employee");
                        case "payroll":
                            return RedirectToAction("Index", "Payroll");
                        case "quanlythongbao":
                            return RedirectToAction("Alerts", "Notification");
                        case "user":
                            return RedirectToAction("Index", "User");
                        default:
                            return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ViewBag.Message = "Invalid username or password.";
                    ViewBag.IsSuccess = false;
                    return View(model);
                }
            }
            catch (Exception)
            {
                ViewBag.Message = "An error occurred during login. Please try again later.";
                ViewBag.IsSuccess = false;
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(UserModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Please enter all valid information.";
                ViewBag.IsSuccess = false;
                return View(model);
            }

            try
            {
                // Check for existing username, email, phone number
                if (_accountDAL.IsUserNameExists(model.UserName))
                {
                    ModelState.AddModelError("UserName", "Username already exists. Please choose another one.");
                    return View(model);
                }

                if (!string.IsNullOrEmpty(model.Email) && _accountDAL.IsEmailExists(model.Email))
                {
                    ModelState.AddModelError("Email", "Email has already been used. Please choose another email.");
                    return View(model);
                }

                if (!string.IsNullOrEmpty(model.Mobile) && _accountDAL.IsMobileExists(model.Mobile))
                {
                    ModelState.AddModelError("Mobile", "Phone number has already been used. Please enter a different number.");
                    return View(model);
                }


                model.IsActive = true;
                model.Role = "user";
                model.IsRemember = false;

                _accountDAL.CreateUser(model);

                TempData["Success"] = "Account created successfully. Please log in.";
                return RedirectToAction("Login");
            }
            catch (SqlException exSql)
            {
                ModelState.AddModelError("", "Username or Email already exists. Please choose different information.");
                return View(model);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error occurred while creating the account. Please try again later.");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        
        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Message = "Vui lòng nhập địa chỉ email.";
                return View();
            }

            bool emailExists = _accountDAL.IsEmailExists(email);
            if (!emailExists)
            {
                ViewBag.Message = "Tài khoản với email này chưa tồn tại.";
                return View();
            }
            TempData["EmailToReset"] = email;
            return RedirectToAction("ResetPassword");
        }
        [HttpGet]
        public ActionResult ResetPassword()
        {
            if (TempData["EmailToReset"] == null)
            {
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Email = TempData["EmailToReset"].ToString();
            return View();
        }


        [HttpPost]
        public ActionResult ResetPassword(string email, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Message = "Vui lòng nhập đầy đủ thông tin.";
                ViewBag.Email = email;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Message = "Mật khẩu xác nhận không khớp.";
                ViewBag.Email = email;
                return View();
            }

            bool result = _accountDAL.ResetPassword(email, newPassword);
            if (result)
            {
                ViewBag.Message = "Mật khẩu đã được cập nhật thành công. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login");
            }
            else
            {
                ViewBag.Message = "Email không tồn tại hoặc có lỗi xảy ra.";
                ViewBag.Email = email;
                return View();
            }
        }
    }
}