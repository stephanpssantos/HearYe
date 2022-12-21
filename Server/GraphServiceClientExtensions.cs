using Azure.Identity;
using Microsoft.Graph;

namespace HearYe.Server
{
    internal static class GraphServiceClientExtensions
    {
        public static IServiceCollection AddGraphClient
            (this IServiceCollection services, string[] scopes, string tenantId, string clientId, string clientSecret)
        {
            TokenCredentialOptions options = new ()
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            ClientSecretCredential clientSecretCredential = new (tenantId, clientId, clientSecret, options);

            services.AddScoped(sp => new GraphServiceClient(clientSecretCredential, scopes));

            return services;
        }
    }
}
