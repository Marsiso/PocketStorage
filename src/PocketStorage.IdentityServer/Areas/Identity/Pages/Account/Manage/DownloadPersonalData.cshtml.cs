using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PocketStorage.Domain.Application.Models;

namespace PocketStorage.IdentityServer.Areas.Identity.Pages.Account.Manage;

public class DownloadPersonalDataModel : PageModel
{
    private readonly ILogger<DownloadPersonalDataModel> _logger;
    private readonly UserManager<User> _userManager;

    public DownloadPersonalDataModel(
        UserManager<User> userManager,
        ILogger<DownloadPersonalDataModel> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public IActionResult OnGet() => NotFound();

    public async Task<IActionResult> OnPostAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", _userManager.GetUserId(User));

        Dictionary<string, string> personalData = new();
        IEnumerable<PropertyInfo> personalDataProps = typeof(User).GetProperties().Where(propertyInfo => Attribute.IsDefined(propertyInfo, typeof(PersonalDataAttribute)));

        foreach (PropertyInfo p in personalDataProps)
        {
            personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
        }

        IList<UserLoginInfo> loginInfos = await _userManager.GetLoginsAsync(user);
        foreach (UserLoginInfo loginInfo in loginInfos)
        {
            personalData.Add($"{loginInfo.LoginProvider} external login provider key", loginInfo.ProviderKey);
        }

        personalData.Add("Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user));

        Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");

        return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), MediaTypeNames.Application.Json);
    }
}
