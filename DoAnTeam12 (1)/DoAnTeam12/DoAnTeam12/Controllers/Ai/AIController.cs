using DoAnTeam12.Services;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DoAnTeam12.Controllers.Ai
{
    public class AIController : Controller
    {
        // GET: AI
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Chat(string prompt)
        {
            AI ai = new AI();
            var response = await ai.GetGPTResponse(prompt);
            ViewBag.Prompt = prompt;
            ViewBag.Response = response;
            return View("Index");
        }
    }
}
