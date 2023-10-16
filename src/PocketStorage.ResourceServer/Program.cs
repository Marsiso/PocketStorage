using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using PocketStorage.Application.Extensions;
using PocketStorage.ResourceServer.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

services.AddOptions();
services.AddSingleton(configuration);
services.AddSingleton(environment);

services.AddApplicationAntiforgery();
services.AddHttpClient();

services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        string authority = configuration["OpenIdConnect:Authority"] ?? string.Empty;
        string clientId = configuration["OpenIdConnect:ClientId"] ?? string.Empty;
        string clientSecret = configuration["OpenIdConnect:ClientSecret"] ?? string.Empty;

        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Authority = authority;
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.RequireHttpsMetadata = true;
        options.ResponseType = OpenIddictConstants.ResponseTypes.Code;
        options.UsePkce = true;
        options.Scope.Add(OpenIddictConstants.Scopes.Profile);
        options.Scope.Add(OpenIddictConstants.Scopes.Email);
        options.Scope.Add(OpenIddictConstants.Scopes.Roles);
        options.Scope.Add(OpenIddictConstants.Scopes.OfflineAccess);
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.RefreshInterval = TimeSpan.FromMinutes(3);
        options.AutomaticRefreshInterval = TimeSpan.FromMinutes(10);
        options.AccessDeniedPath = "/";
        options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = OpenIddictConstants.Claims.Name };
    });

services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

services.AddRazorPages()
    .AddMvcOptions(options => options.Configure());

WebApplication application = builder.Build();

if (application.Environment.IsDevelopment())
{
    application.UseDeveloperExceptionPage();
    application.UseWebAssemblyDebugging();
}
else
{
    application.UseHsts();
}

application.UseSecurityHeaders(SecurityHeadersHelpers.GetHeaderPolicyCollection(environment.IsDevelopment(), configuration));

application.UseHttpsRedirection();
application.UseBlazorFrameworkFiles();
application.UseStaticFiles();

application.UseRouting();
application.UseNoUnauthorizedRedirect("/api");
application.UseAuthentication();
application.UseAuthorization();

application.MapRazorPages();
application.MapControllers();
application.MapNotFound("/api/{**segment}");
application.MapFallbackToPage("/_Host");

application.Run();
