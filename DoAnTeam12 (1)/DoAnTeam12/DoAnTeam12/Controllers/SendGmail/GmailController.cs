using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnTeam12.Models.SendEmail;

namespace DoAnTeam12.Controllers.SendGmail
{
    public class GmailController : Controller
    {
        // GET: Gmail
        public ActionResult Send()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Send(Gmail gmail)
        {
            gmail.SendEmail();
            return View();
        }
    }
}