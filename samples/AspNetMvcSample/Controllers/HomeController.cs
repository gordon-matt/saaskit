using System.Diagnostics;
using AspNetMvcSample.Models;
using Microsoft.AspNetCore.Mvc;
using SaasKit.Multitenancy;

namespace AspNetMvcSample.Controllers;

public class HomeController : Controller
{
    private readonly AppTenant tenant;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ITenant<AppTenant> tenant, ILogger<HomeController> logger)
    {
        this.tenant = tenant?.Value;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult About()
    {
        ViewData["Message"] = $"Your application description page for {tenant?.Name ?? "Default"}";

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