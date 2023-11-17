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
    .AddOidcAuthentication()
    .AddCookie(options => { })
    .AddOidc(configuration);

services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

services
    .AddRazorPages()
    .AddMvcOptions(options => options.Configure());

services.AddTransient<ISaveChangesInterceptor, AuditTrailInterceptor>();
services.AddDbContext<DataContext>(options => options.Configure(environment.IsDevelopment(), configuration));
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
