using System.Net.Http.Headers;
using System.Net.Mime;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MudBlazor.Services;
using PocketStorage.BFF.Authorization.Constants;
using PocketStorage.BFF.Authorization.Extensions;
using PocketStorage.Client;
using PocketStorage.Client.Services;
using PocketStorage.Client.Services.Contracts;
using PocketStorage.Integration;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

IServiceCollection services = builder.Services;
IConfiguration configuration = builder.Configuration;

services.AddOptions();
services.AddSingleton(configuration);

services.AddAuthorizationCore();

services.TryAddSingleton<AuthenticationStateProvider, HostAuthenticationStateProvider>();
services.TryAddSingleton(sp => (HostAuthenticationStateProvider)sp.GetRequiredService<AuthenticationStateProvider>());
services.AddTransient<AuthorizedHandler>();
services.AddPermissionAuthorization();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

services.AddMudServices();

services.AddHttpClient("default", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
});

services.AddHttpClient(AuthorizationConstants.AuthorizedClientName, client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
}).AddHttpMessageHandler<AuthorizedHandler>();

services.AddTransient(serviceProvider => serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient("default"));
services.AddTransient<IAntiforgeryHttpClientFactory, AntiforgeryHttpClientFactory>();

services.AddTransient<NSwagClient>();
services.AddTransient<PocketStorageClient>();

await builder.Build().RunAsync();
