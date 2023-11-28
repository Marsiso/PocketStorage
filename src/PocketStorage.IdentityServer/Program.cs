using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PocketStorage.AppHost.ServiceDefaults;
using PocketStorage.Application.Application.Mappings;
using PocketStorage.Application.Application.Validators;
using PocketStorage.Application.BackgroundServices;
using PocketStorage.Application.Extensions;
using PocketStorage.Application.Services;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Core.Pipelines;
using PocketStorage.Data;
using PocketStorage.Data.Interceptors;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? solutionLocation = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.FullName ?? throw new InvalidOperationException();
IConfigurationRoot globalSettings = new ConfigurationBuilder()
    .SetBasePath(solutionLocation)
    .AddJsonFile("appsettings.json")
    .Build();

ApplicationSettings applicationSettings = globalSettings.GetSection(ApplicationSettings.SectionName).Get<ApplicationSettings>() ?? throw new InvalidOperationException();

builder.AddServiceDefaults();
builder.AddRedisOutputCache("redis-cache");

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

services.AddRazorPages();
services.AddControllersWithViews();

services.AddOptions();
services.AddSingleton(configuration);
services.AddSingleton(applicationSettings);

services.AddOptions<ArgonPasswordHasherOptions>()
    .Configure(options => { options.Pepper = "SecurePasswordPepper"; })
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddSingleton<IPasswordHasher<User>, ArgonPasswordHasher<User>>();

services.AddTransient<ISaveChangesInterceptor, AuditTrailInterceptor>();
services.AddDbContext<DataContext>(options => options.Configure(environment.IsDevelopment(), applicationSettings));
services.AddDatabaseDeveloperPageExceptionFilter();

services.AddIdentity<User, Role>(options => options.Configure())
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = TimeSpan.FromHours(1));
services.ConfigureApplicationCookie(options => options.Configure());

services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<DataContext>();
    })
    .AddServer(builder =>
    {
        builder.SetIssuer(applicationSettings.OpenIdConnect.Server.Authority);

        // Enable the authorization, logout, token and user info endpoints.
        builder.SetAuthorizationEndpointUris("/connect/authorize");
        builder.SetLogoutEndpointUris("/connect/logout");
        builder.SetTokenEndpointUris("/connect/token");
        builder.SetUserinfoEndpointUris("/connect/userInfo");
        builder.SetIntrospectionEndpointUris("/connect/introspect");
        builder.SetVerificationEndpointUris("/connect/verify");

        // Mark the "profile.name", "profile.email", ... as supported scopes.
        builder.RegisterScopes(applicationSettings.OpenIdConnect.Server.SupportedScopes.ToArray());

        // Mark the "name", "given_name", ... as supported claims.
        builder.RegisterClaims(applicationSettings.OpenIdConnect.Server.SupportedClaims.ToArray());

        // Enable the client credentials flow
        builder.AllowClientCredentialsFlow();
        builder.AllowAuthorizationCodeFlow();
        builder.AllowRefreshTokenFlow();
        builder.RequireProofKeyForCodeExchange();

        // Register the signing and encryption credentials.
        builder.AddDevelopmentEncryptionCertificate();
        builder.AddDevelopmentSigningCertificate();

        // Register the ASP.NET Core host and configure the ASP.NET Core options.
        OpenIddictServerAspNetCoreBuilder serverBuilder = builder.UseAspNetCore()
            .EnableAuthorizationEndpointPassthrough()
            .EnableLogoutEndpointPassthrough()
            .EnableTokenEndpointPassthrough()
            .EnableUserinfoEndpointPassthrough()
            .EnableStatusCodePagesIntegration();

        if (environment.IsDevelopment())
        {
            serverBuilder.DisableTransportSecurityRequirement();
        }
    })
    .AddValidation(options =>
    {
        // Import the configuration from the local OpenIddict server instance.
        options.UseLocalServer();

        // Register the ASP.NET Core host.
        options.UseAspNetCore();
    });

services.AddAutoMapper(typeof(UserProfile));
services.AddValidatorsFromAssembly(typeof(LoginInputValidator).Assembly);
services.AddMediatR(options =>
{
    options.RegisterServicesFromAssembly(typeof(GetUserQuery).Assembly);
    options.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RequestPipelineBehaviour<,>));
});

services.AddHostedService<Worker>();

WebApplication application = builder.Build();

if (environment.IsDevelopment())
{
    application.UseCors(options =>
    {
        options.AllowAnyHeader();
        options.AllowAnyMethod();
        options.AllowAnyOrigin();
    });

    application.UseMigrationsEndPoint();
}
else
{
    application.UseExceptionHandler("/Home/Error");
    application.UseHsts();
    application.UseHttpsRedirection();
}

application.UseStaticFiles();
application.UseRouting();

application.UseAuthentication();
application.UseAuthorization();

application.MapDefaultEndpoints();

application.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

application.MapRazorPages();

application.UseOutputCache();

application.Run();

namespace PocketStorage.IdentityServer
{
    public partial class Program
    {
        public static Program CreateInstance() => new();
    }
}
