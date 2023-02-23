using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace HearYe.Client
{
    public class CustomAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public CustomAuthorizationMessageHandler(IAccessTokenProvider provider, 
            NavigationManager navigation, string APIBaseURI) : base(provider, navigation) 
        {
            ConfigureHandler(authorizedUrls: new[] { APIBaseURI });
        }
    }
}
