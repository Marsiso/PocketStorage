using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PocketStorage.Client;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

services.AddHttpClient("PocketStorage.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));

services.AddScoped(serviceProvider => serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("PocketStorage.ServerAPI"));

await builder.Build().RunAsync();
