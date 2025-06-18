using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ClinicalResearchApp.Models;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System.Security.Claims;
using Microsoft.Extensions.Options; // Required for ClaimTypes


namespace ClinicalResearchApp.Controllers;


public class HomeController : Controller
{   
    [Authorize]
    public IActionResult Index()
    {

        string userName = User.Identity.IsAuthenticated 
            ? User.Claims.FirstOrDefault(c => c.Type == "name")?.Value 
            : "Guest";

        return RedirectToAction("Index", "Research");
    }

    [Authorize]
    public IActionResult Profile()
    {
        string userName = User.Identity.IsAuthenticated 
            ? User.Claims.FirstOrDefault(c => c.Type == "name")?.Value 
            : "Guest";

        ViewBag.Username = userName;
        
        foreach (var claim in User.Claims)
        {
            Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
        }

        return View();
    }

    public IActionResult Login()
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = "/signin-oidc"
        }, OpenIdConnectDefaults.AuthenticationScheme);
    }

    public IActionResult Logout()
    {
        return SignOut("Cookies", "OpenIdConnect");
    }

    public IActionResult Error()
    {
        var errorMessage = HttpContext.Items["ErrorMessage"]?.ToString() ?? "An unknown error occurred.";
        var errorDetails = HttpContext.Items["ErrorDetails"]?.ToString() ?? "No additional details available.";

        var model = new ErrorViewModel
        {
            ErrorMessage = errorMessage,
            ErrorDetails = errorDetails
        };

        return View(model);
    }

    [Authorize]
    [HttpGet]
    public IActionResult GetUserEmail()
    {
        var email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ??
                    User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return Json(new { success = false, email = "" });
        }

        return Json(new { success = true, email });
    }
}
