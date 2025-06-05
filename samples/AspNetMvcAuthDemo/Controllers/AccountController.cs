using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMvcAuthSample.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> logger;

    public AccountController(ILogger<AccountController> logger)
    {
        this.logger = logger;
    }

    public IActionResult LogIn() => View();

    public IActionResult Google()
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = "/home/about"
        };

        return new ChallengeResult("Google", props);
    }

    public async Task<IActionResult> LogOut()
    {
        await HttpContext.SignOutAsync("Cookies");

        return RedirectToAction("index", "home");
    }
}