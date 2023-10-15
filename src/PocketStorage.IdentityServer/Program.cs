WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

services.AddControllersWithViews();

WebApplication application = builder.Build();

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
