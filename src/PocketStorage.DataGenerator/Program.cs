using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PocketStorage.Application.Extensions;
using PocketStorage.Application.Services;
using PocketStorage.Data;
using PocketStorage.Data.Interceptors;
using PocketStorage.DataGenerator.Services;
using PocketStorage.Domain.Application.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

services.AddOptions();
services.AddSingleton(configuration);

services.AddOptions<ArgonPasswordHasherOptions>()
    .Configure(options => { options.Pepper = "SecurePasswordPepper"; })
    .ValidateDataAnnotations()
    .ValidateOnStart();

services.AddSingleton<IPasswordHasher<User>, ArgonPasswordHasher<User>>();

services.AddTransient<ISaveChangesInterceptor, AuditTrailInterceptor>();
services.AddDbContext<DataContext>(options => options.Configure(environment.IsDevelopment(), configuration));

services.AddIdentity<User, Role>(options => options.Configure())
    .AddEntityFrameworkStores<DataContext>();

services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<DataContext>();
    });

services.AddHostedService<UserSeedService>();
services.AddHostedService<ClientSeedService>();

WebApplication application = builder.Build();

application.Run();
