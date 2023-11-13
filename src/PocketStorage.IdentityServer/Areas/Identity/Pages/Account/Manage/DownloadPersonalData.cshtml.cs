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
    private readonly UserManager<User> _userManager;

    public DownloadPersonalDataModel(UserManager<User> userManager) => _userManager = userManager;

    public IActionResult OnGet() => NotFound();

    public async Task<IActionResult> OnPostAsync()
    {
        User? user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        IEnumerable<PropertyInfo> personalDataProps = typeof(User).GetProperties().Where(propertyInfo => Attribute.IsDefined(propertyInfo, typeof(PersonalDataAttribute)));

        Dictionary<string, string> personalData = personalDataProps.ToDictionary(p => p.Name, p => p.GetValue(user)?.ToString() ?? "null");

        IList<UserLoginInfo> logins = await _userManager.GetLoginsAsync(user);
        foreach (UserLoginInfo login in logins)
        {
            personalData.Add($"{login.LoginProvider} external login provider key", login.ProviderKey);
        }

        personalData.Add("Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user));

        Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");

        return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), MediaTypeNames.Application.Json);
    }
}
