using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

WebApplication application = builder.Build();

using IServiceScope serviceScope = application.Services.CreateScope();

DataContext databaseContext = serviceScope.ServiceProvider.GetRequiredService<DataContext>();

if (environment.IsDevelopment())
{
    databaseContext.Database.EnsureDeleted();
}

databaseContext.Database.EnsureCreated();

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
