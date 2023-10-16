using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OpenIddict.Abstractions;
using PocketStorage.Application.Extensions;
using PocketStorage.Data;
using PocketStorage.Data.Interceptors;
using PocketStorage.Domain.Application.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

services.AddRazorPages();
services.AddControllersWithViews();

services.AddSingleton(configuration);

services.AddTransient<ISaveChangesInterceptor, AuditTrailInterceptor>();
services.AddDbContext<DataContext>(options => options.Configure(environment.IsDevelopment(), configuration));
services.AddDatabaseDeveloperPageExceptionFilter();

services.AddIdentity<User, Role>(options => options.Configure())
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(1));
services.ConfigureApplicationCookie(options => options.Configure());

/*services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = configuration["Clients:Google:Id"] ?? string.Empty;
        options.ClientSecret = configuration["Clients:Google:Secret"] ?? string.Empty;
    })
    .AddFacebook(options =>
    {
        options.ClientId = configuration["Clients:Facebook:Id"] ?? string.Empty;
        options.ClientSecret = configuration["Clients:Facebook:Secret"] ?? string.Empty;
    })
    .AddMicrosoftAccount(options =>
    {
        options.ClientId = configuration["Clients:MicrosoftAccount:Id"] ?? string.Empty;
        options.ClientSecret = configuration["Clients:MicrosoftAccount:Secret"] ?? string.Empty;
    });*/

services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<DataContext>();
    })
    .AddServer(builder =>
    {
        // Enable the authorization, logout, token and user info endpoints.
        builder.SetAuthorizationEndpointUris("/connect/authorize");
        builder.SetLogoutEndpointUris("/connect/logout");
        builder.SetTokenEndpointUris("/connect/token");
        builder.SetUserinfoEndpointUris("/connect/userInfo");
        builder.SetIntrospectionEndpointUris("/connect/introspect");
        builder.SetVerificationEndpointUris("/connect/verify");

        // Mark the "email", "profile" and "roles" scopes as supported scopes.
        builder.RegisterScopes(OpenIddictConstants.Scopes.Email, OpenIddictConstants.Scopes.Profile, OpenIddictConstants.Scopes.Roles);

        // Enable the client credentials flow
        builder.AllowClientCredentialsFlow();
        builder.AllowAuthorizationCodeFlow();
        builder.AllowRefreshTokenFlow();
        builder.RequireProofKeyForCodeExchange();

        // Register the signing and encryption credentials.
        builder.AddDevelopmentEncryptionCertificate();
        builder.AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core options.
        builder.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableLogoutEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserinfoEndpointPassthrough()
            .EnableStatusCodePagesIntegration();
    })
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

WebApplication application = builder.Build();

if (environment.IsDevelopment())
{
    application.UseMigrationsEndPoint();
}
else
{
    application.UseExceptionHandler("/Home/Error");
    application.UseHsts();
}

application.UseHttpsRedirection();
application.UseStaticFiles();

application.UseRouting();

application.UseAuthentication();
application.UseAuthorization();

application.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

application.MapRazorPages();

application.Run();
