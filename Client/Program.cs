using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HearYe.Client;
using HearYe.Client.Data;
using Microsoft.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string apiURI = string.Empty;
if (builder.HostEnvironment.IsDevelopment())
{
    apiURI = builder.Configuration["APIDevURI"]!;
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}
else
{
    apiURI = builder.Configuration["APIBaseURI"]!;
    builder.Logging.SetMinimumLevel(LogLevel.Warning);
}

builder.Services.AddTransient(sp => 
{ 
    return new CustomAuthorizationMessageHandler(
        sp.GetRequiredService<IAccessTokenProvider>(), 
        sp.GetRequiredService<NavigationManager>(),
        apiURI);
});
builder.Services.AddHttpClient("HearYe.ServerAPI", client => client.BaseAddress = new Uri(apiURI))
    .AddHttpMessageHandler<CustomAuthorizationMessageHandler>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler(Policies.GetRetryPolicy());

builder.Services.AddHttpClient("HearYe.ServerAPI.HealthCheck", client => client.BaseAddress = new Uri(apiURI))
    .SetHandlerLifetime(TimeSpan.FromMinutes(3))
    .AddPolicyHandler(Policies.GetRetryPolicy());

/* BaseAddressAuthorizationMessageHandler will not send auth claims to the server address unless it matches the client's
   address. Therefore a custom message handler (with the API's server address configured) must be used. */

// builder.Services.AddHttpClient("HearYe.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
//    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>()
//    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
//    .AddPolicyHandler(Policies.GetRetryPolicy());

// Supply HttpClient instances that include access tokens when making requests to the server project
// In Blazor, Scoped behaves like a singleton i.e. a single instance is created and shared.
// Transient means you get a new instance of the service every time its requested.
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("HearYe.ServerAPI"));
builder.Services.AddTransient<IHearYeService, HearYeService>();
builder.Services.AddSingleton<StateContainer>();

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add(builder.Configuration["AzureTokenScope"] !);
    options.ProviderOptions.LoginMode = "redirect"; // Popup(default) or login.
    options.ProviderOptions.Cache.CacheLocation = "localStorage"; // Cache auth token. Local or session(default).
    options.AuthenticationPaths.LogOutSucceededPath = "/landing";
});

await builder.Build().RunAsync();
