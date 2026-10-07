using System.Diagnostics;
using ConfigApi.Mvc.Models;
using Microsoft.AspNetCore.Mvc;

namespace ConfigApi.Mvc.Controllers;

public class HomeController : Controller
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
