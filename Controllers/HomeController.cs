using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using JobPortal.Models;

namespace JobPortal.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // If user is admin, redirect to dashboard
        if (User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Admin"))
        {
            return RedirectToAction("Dashboard", "Admin");
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
