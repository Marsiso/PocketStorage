using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using OpenIddict.Abstractions;
using PocketStorage.Application.Application.Mappings;
using PocketStorage.Application.Application.Validators;
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
    .AddJsonFile("global.json")
    .Build();

ApplicationSettings applicationSettings = globalSettings.GetSection(ApplicationSettings.SectionName).Get<ApplicationSettings>() ?? throw new InvalidOperationException();

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

services.AddOptions();
services.AddSingleton(configuration);
services.AddSingleton(environment);
services.AddSingleton(applicationSettings);

services.AddAntiforgery(options => options.Configure());
services.AddHttpClient();

services
    .AddAuthentication(options => options.Configure())
    .AddCookie()
    .AddOpenIdConnect(options => options.Configure(applicationSettings));

services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

services
    .AddRazorPages()
    .AddMvcOptions(options => options.Configure());

services.AddTransient<ISaveChangesInterceptor, AuditTrailInterceptor>();
services.AddDbContext<DataContext>(options => options.Configure(environment.IsDevelopment(), applicationSettings));
services.AddDatabaseDeveloperPageExceptionFilter();

services.AddSingleton<IPasswordHasher<User>, ArgonPasswordHasher<User>>();

services.AddIdentityCore<User>()
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

builder.Services.AddOpenApiDocument(options =>
{
    options.PostProcess = document =>
    {
        document.Info = new OpenApiInfo
        {
            Version = "v1",
            Title = "Pocket Storage Resource Server",
            Description = "An ASP.NET Core RESTful API that is part of the BFF pattern for the Blazor Web Assembly client application.",
            TermsOfService = "https://localhost:5003/privacy",
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
                    TokenUrl = "https://localhost:5001/connect/token",
                    AuthorizationUrl = "https://localhost:5001/connect/authorize",
                    Scopes = new Dictionary<string, string>(applicationSettings.OpenIdConnect.Clients.Single(client => client.Id == "pocket_storage_resource_server_swagger").Scopes.ToDictionary(scope => scope, _ => "Scope description"))
                    {
                        [OpenIddictConstants.Scopes.OpenId] = "Scope description"
                    }
                }
            }
        });

    options.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("OAuth2"));
});

WebApplication application = builder.Build();

if (application.Environment.IsDevelopment())
{
    application.UseDeveloperExceptionPage();
    application.UseWebAssemblyDebugging();

    application.UseOpenApi();
    application.UseSwaggerUi(options =>
    {
        options.DocumentTitle = "Pocket Storage Resource Server";

        OpenIdConnectClientSettings client = applicationSettings.OpenIdConnect.Clients.Single(client => client.Id == "pocket_storage_resource_server_swagger");
        options.OAuth2Client = new OAuth2ClientSettings { AppName = client.DisplayName, ClientId = client.Id, ClientSecret = client.Secret, UsePkceWithAuthorizationCodeGrant = true };
    });

    application.UseReDoc(options => options.Path = "/redoc");
}

application.UseSecurityHeaders(SecurityHeadersHelpers.GetHeaderPolicyCollection(environment.IsDevelopment(), applicationSettings));

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
