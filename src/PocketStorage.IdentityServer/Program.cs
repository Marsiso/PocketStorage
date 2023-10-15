using Microsoft.EntityFrameworkCore.Diagnostics;
using PocketStorage.Application.Extensions;
using PocketStorage.Data;
using PocketStorage.Data.Interceptors;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

services.AddControllersWithViews();
services.AddTransient<ISaveChangesInterceptor, AuditTrailInterceptor>();
services.AddDbContext<DataContext>(options => options.Configure(environment.IsDevelopment(), configuration));

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
}
else
{
    application.UseExceptionHandler("/Home/Error");
    application.UseHsts();
}

application.UseHttpsRedirection();
application.UseStaticFiles();

application.UseRouting();

application.UseAuthorization();

application.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

application.Run();
