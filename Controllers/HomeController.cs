using Microsoft.AspNetCore.Mvc;

namespace QLDuLichRBAC_Upgrade.Controllers
{
	public class HomeController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}
	}
}
