using Microsoft.AspNetCore.Mvc;

namespace TennisCourtRentalSystem.Controllers;
public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
