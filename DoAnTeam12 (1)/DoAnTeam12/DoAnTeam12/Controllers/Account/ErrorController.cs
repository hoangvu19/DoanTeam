using System.Web.Mvc;

namespace DoAnTeam12.Controllers
{
    public class ErrorController : Controller
    {
        
        public ActionResult AccessDenied()
        {
            Response.StatusCode = 403; 
            Response.SuppressFormsAuthenticationRedirect = true; 

            return View(); 
        }

        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            Response.SuppressFormsAuthenticationRedirect = true;
            return View();
        }

        public ActionResult ServerError()
        {
            Response.StatusCode = 500;
            Response.SuppressFormsAuthenticationRedirect = true;
            return View();
        }
    }
}