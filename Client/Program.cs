using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using HearYe.Client;
using HearYe.Client.Data;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("HearYe.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("HearYe.ServerAPI"));
builder.Services.AddTransient<IHearYeService, HearYeService>();

builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("https://StephansDomain.onmicrosoft.com/148c352e-fae8-4c4b-b98f-bf2a440f3428/access_as_user");
    options.ProviderOptions.LoginMode = "redirect"; // Popup(default) or login.
    options.ProviderOptions.Cache.CacheLocation = "localStorage"; // Local or session(default).
});

await builder.Build().RunAsync();
