using System.Web.Mvc;
using AspNet.Identity.AdoNetProvider.WebUI.Infrastructure;

namespace AspNet.Identity.AdoNetProvider.WebUI.Controllers
{
    [NoCache]
    [Authorize]
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
    }
}