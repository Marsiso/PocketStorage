using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

services.AddAutoMapper(typeof(UserProfile));
services.AddValidatorsFromAssembly(typeof(LoginInputValidator).Assembly);
services.AddMediatR(options =>
{
    options.RegisterServicesFromAssembly(typeof(GetUserQuery).Assembly);
    options.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RequestPipelineBehaviour<,>));
});

WebApplication application = builder.Build();

if (application.Environment.IsDevelopment())
{
    application.UseDeveloperExceptionPage();
    application.UseWebAssemblyDebugging();
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
