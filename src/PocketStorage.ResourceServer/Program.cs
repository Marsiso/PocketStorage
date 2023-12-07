using System.Globalization;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using OpenIddict.Abstractions;
using PocketStorage.AppHost.ServiceDefaults;
using PocketStorage.Application.Application.Mappings;
using PocketStorage.Application.Application.Validators;
using PocketStorage.Application.Extensions;
using PocketStorage.Application.Helpers;
using PocketStorage.Application.Services;
using PocketStorage.BFF.Authorization.Constants;
using PocketStorage.BFF.Authorization.Extensions;
using PocketStorage.Client;
using PocketStorage.Core.Application.Queries;
using PocketStorage.Core.Pipelines;
using PocketStorage.Data;
using PocketStorage.Data.Interceptors;
using PocketStorage.Domain.Application.Models;
using PocketStorage.Domain.Constants;
using PocketStorage.Domain.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string solutionLocation = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.FullName ?? throw new InvalidOperationException();
IConfigurationRoot globalSettings = new ConfigurationBuilder()
    .SetBasePath(solutionLocation)
    .AddJsonFile("appsettings.json")
    .Build();

Settings settings = globalSettings.GetSection(Settings.SectionName).Get<Settings>() ?? throw new InvalidOperationException();

builder.AddServiceDefaults();
builder.AddRedisOutputCache("redis-cache");

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

services.AddOptions();
services.AddSingleton(configuration);
services.AddSingleton(environment);
services.AddSingleton(settings);
services.AddAntiforgery(options => options.Configure(environment.IsDevelopment()));
services.AddHttpClient();

services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options => { options.Cookie.Name = "__RESOURCE-SERVER-COOKIE"; })
    .AddOpenIdConnect(options =>
    {
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.Authority = settings.OpenIdConnect.Server.Authority;
        options.ClientId = settings.OpenIdConnect.Clients.Single(client => client.ClientId == "pocket_storage_resource_server").ClientId;
        options.ClientSecret = settings.OpenIdConnect.Clients.Single(client => client.ClientId == "pocket_storage_resource_server").ClientSecret;

        options.UsePkce = true;
        options.ResponseType = OpenIddictConstants.ResponseTypes.Code;
        options.RequireHttpsMetadata = !environment.IsDevelopment();

        options.Scope.Add(OpenIddictConstants.Scopes.Profile);
        options.Scope.Add(OpenIddictConstants.Scopes.Roles);
        options.Scope.Add(OpenIddictScopeDefaults.Name);
        options.Scope.Add(OpenIddictScopeDefaults.Email);
        options.Scope.Add(OpenIddictScopeDefaults.PhoneNumber);
        options.Scope.Add(OpenIddictScopeDefaults.Locale);
        options.Scope.Add(OpenIddictScopeDefaults.Zoneinfo);
        options.Scope.Add(OpenIddictScopeDefaults.UpdatedAt);
        options.Scope.Add(PermitConstants.Scopes.Permissions);
        options.Scope.Add(OpenIddictConstants.Scopes.OfflineAccess);

        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.RefreshInterval = TimeSpan.FromMinutes(3);
        options.AutomaticRefreshInterval = TimeSpan.FromMinutes(10);

        options.AccessDeniedPath = "/";
        options.TokenValidationParameters = new TokenValidationParameters { NameClaimType = OpenIddictConstants.Claims.Name };
    });

services.AddPermissionAuthorization();
services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
services.AddLocalization(options => options.ResourcesPath = "Resources");
services.AddTransient<IStringLocalizer, StringLocalizer<App>>();

services
    .AddRazorPages()
    .AddMvcOptions(options => options.Filters.Add(new AuthorizeFilter(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())));

services.AddTransient<ISaveChangesInterceptor, AuditTrailInterceptor>();
services.AddDbContext<DataContext>(options => options.Configure(environment.IsDevelopment(), settings));
services.AddDatabaseDeveloperPageExceptionFilter();
services.AddSingleton<IPasswordHasher<User>, ArgonPasswordHasher<User>>();

services
    .AddIdentityCore<User>()
    .AddRoles<Role>()
    .AddEntityFrameworkStores<DataContext>();

services.AddHttpContextAccessor();
services.AddAutoMapper(typeof(UserProfile));
services.AddValidatorsFromAssembly(typeof(LoginInputValidator).Assembly);
services.AddMediatR(options =>
{
    options.RegisterServicesFromAssembly(typeof(GetUserQuery).Assembly);
    options.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RequestPipelineBehaviour<,>));
});

services.AddEndpointsApiExplorer();
services.AddOpenApiDocument(options =>
{
    options.PostProcess = document =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "v1",
            Title = "Pocket Storage Resource Server",
            Description = "An ASP.NET Core RESTful API that is part of the BFF pattern for the Blazor Web Assembly client application.",
            TermsOfService = "http://localhost:5002/privacy",
            Contact = new OpenApiContact { Name = "LinkedIn", Url = "https://www.linkedin.com/in/marek-ol%C5%A1%C3%A1k-1715b724a/" },
            License = new OpenApiLicense { Name = "MIT", Url = "https://en.wikipedia.org/wiki/MIT_License" }
        };
    };

    options.AddSecurity("OAuth2", Enumerable.Empty<string>(),
        new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    TokenUrl = "http://localhost:5000/connect/token",
                    AuthorizationUrl = "http://localhost:5000/connect/authorize",
                    Scopes = new Dictionary<string, string>(settings.OpenIdConnect.Clients.Single(client => client.ClientId == "pocket_storage_resource_server_swagger").Scopes.ToDictionary(scope => scope, _ => "Scope description"))
                    {
                        [OpenIddictConstants.Scopes.OpenId] = "Scope description"
                    }
                }
            }
        });

    options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("OAuth2"));
});

WebApplication application = builder.Build();

application.UseSecurityHeaders(SecurityHeadersHelpers.GetHeaderPolicyCollection(environment.IsDevelopment(), settings));
application.UseRequestLocalization(options =>
{
    CultureInfo[] supportedCultures = { new(CultureDefaults.English), new(CultureDefaults.Czech) };

    options.DefaultRequestCulture = new RequestCulture(CultureDefaults.Default);
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

if (environment.IsDevelopment())
{
    application.UseDeveloperExceptionPage();
    application.UseWebAssemblyDebugging();
    application.UseOpenApi();
    application.UseSwaggerUi(options =>
    {
        options.DocumentTitle = "Pocket Storage Resource Server";

        OpenIdConnectClientSettings client = settings.OpenIdConnect.Clients.Single(client => client.ClientId == "pocket_storage_resource_server_swagger");
        options.OAuth2Client = new OAuth2ClientSettings { AppName = client.DisplayName, ClientId = client.ClientId, ClientSecret = client.ClientSecret, UsePkceWithAuthorizationCodeGrant = true };
    });

    application.UseReDoc(options => options.Path = "/redoc");
}
else
{
    application.UseHttpsRedirection();
}

application.UseBlazorFrameworkFiles();
application.UseStaticFiles();
application.UseRouting();
application.UseNoUnauthorizedRedirect("/api");
application.UseAuthentication();
application.UseAuthorization();


application.MapDefaultEndpoints();
application.MapRazorPages();
application.MapControllers();
application.MapNotFound("/api/{**segment}");
application.MapFallbackToPage("/_Host");

application.UseOutputCache();

application.Run();
