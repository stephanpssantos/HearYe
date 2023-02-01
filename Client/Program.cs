using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HearYe.Client;
using HearYe.Client.Data;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("HearYe.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler(Policies.GetRetryPolicy());

// Supply HttpClient instances that include access tokens when making requests to the server project
// In Blazor, Scoped behaves like a singleton i.e. a single instance is created and shared.
// Transient means you get a new instance of the service every time its requested.
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("HearYe.ServerAPI"));
builder.Services.AddTransient<IHearYeService, HearYeService>();
builder.Services.AddSingleton<StateContainer>();

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("https://StephansDomain.onmicrosoft.com/5bcd9a8a-252d-4c87-af83-56d0638a4356/API.Access");
    options.ProviderOptions.LoginMode = "redirect"; // Popup(default) or login.
    options.ProviderOptions.Cache.CacheLocation = "localStorage"; // Cache auth token. Local or session(default).
    options.AuthenticationPaths.LogOutSucceededPath = "/landing";
});

await builder.Build().RunAsync();
