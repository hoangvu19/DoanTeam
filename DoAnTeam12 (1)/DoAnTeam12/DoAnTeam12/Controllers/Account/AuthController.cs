using DoAnTeam12.DAL;
using DoAnTeam12.Models.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnTeam12.Controllers.Account
{
    public class AuthController : Controller
    {
        private AuthDAL authDAL = new AuthDAL();

        public ActionResult Index()
        {
            List<UserModel> userList = new List<UserModel>();
            try
            {
                userList = authDAL.GetTotal();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error loading account list. Please try again later.");
                userList = new List<UserModel>();
            }

            return View(userList);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            UserModel user = null;
            try
            {
                user = authDAL.GetById(id.Value);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error loading account details. Please try again later.");
                return View("Error");
            }


            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserModel user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ModelState.AddModelError("", "Account creation functionality is not fully implemented.");
                    return View(user);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error creating account: " + ex.Message);
                    return View(user);
                }
            }
            return View(user);
        }


        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            UserModel user = null;
            try
            {
                user = authDAL.GetById(id.Value);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error loading account information for editing. Please try again later.");
                return View("Error");
            }


            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserModel user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    authDAL.UpdateAccount(user);

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error updating account: " + ex.Message);
                    return View(user);
                }
            }
            return View(user);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            UserModel user = null;
            try
            {
                user = authDAL.GetById(id.Value);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error loading account information for deletion. Please try again later.");
                return View("Error");
            }


            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            try
            {
                authDAL.DeleteAccount(id);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                UserModel user = null;
                try
                {
                    user = authDAL.GetById(id);
                }
                catch (Exception getEx)
                {
                    ModelState.AddModelError("", "Error deleting account and unable to reload information.");
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", "Error deleting account: " + ex.Message);
                return View("Delete", user);
            }
        }
    }
}
