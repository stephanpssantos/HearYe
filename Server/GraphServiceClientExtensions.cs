// <copyright file="GraphServiceClientExtensions.cs" company="Stephan Santos">
// Copyright (c) Stephan Santos. All rights reserved.
// </copyright>

using Azure.Identity;
using Microsoft.Graph;

namespace HearYe.Server
{
    /// <summary>
    /// Service collection extension to access Graph API.
    /// </summary>
    internal static class GraphServiceClientExtensions
    {
        /// <summary>
        /// Add graph configured client to IServiceCollection.
        /// </summary>
        /// <param name="services">IServiceCollection.</param>
        /// <param name="scopes">Scopes requested by Graph client.</param>
        /// <param name="tenantId">Tenant id of the Azure AD B2C instance.</param>
        /// <param name="clientId">App registration id in Azure.</param>
        /// <param name="clientSecret">Client secret.</param>
        /// <returns>Configured Graph service client.</returns>
        public static IServiceCollection AddGraphClient (
            this IServiceCollection services, string[] scopes, string tenantId, string clientId, string clientSecret)
        {
            TokenCredentialOptions options = new ()
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };

            ClientSecretCredential clientSecretCredential = new (tenantId, clientId, clientSecret, options);

            services.AddScoped(sp => new GraphServiceClient(clientSecretCredential, scopes));

            return services;
        }
    }
}
