﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PocketStorage.IdentityServer.Controllers.Common;
using PocketStorage.IdentityServer.Models;

namespace PocketStorage.IdentityServer.Controllers;

public class HomeController : WebControllerBase<HomeController>
{
    public IActionResult Index() => View();

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
