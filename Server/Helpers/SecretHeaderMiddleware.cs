// <copyright file="SecretHeaderMiddleware.cs" company="Stephan Santos">
// Copyright (c) Stephan Santos. All rights reserved.
// </copyright>

using Azure.Core;
using Microsoft.AspNetCore.Authorization;

namespace HearYe.Server
{
    /// <summary>
    /// Require secret header value for all requests.
    /// API management consumption tier cannot have a constant IP address or be added to
    /// a network. Therefore network access to this application cannot be restricted to
    /// just API management. To ensure all requests come from API management, this header
    /// will be injected into requests by API management, and checked by the application.
    /// </summary>
    public class SecretHeaderMiddleware
    {
        private readonly RequestDelegate next;
        private readonly string secret;
        private readonly string secretKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecretHeaderMiddleware"/> class.
        /// </summary>
        /// <param name="next">Request delegate.</param>
        /// <param name="secret">The expected secret value.</param>
        /// <param name="secretKey">The header key containing the secret.</param>
        public SecretHeaderMiddleware(RequestDelegate next, string secret, string secretKey)
        {
            this.next = next;
            this.secret = secret;
            this.secretKey = secretKey;
        }

        /// <summary>
        /// Check secret/secretKey values.
        /// </summary>
        /// <param name="context">HttpContext.</param>
        /// <returns>The next middleware queued or a 401 response.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey(this.secretKey))
            {
                context.Response.StatusCode = 401;
            }
            else if (context.Request.Headers[this.secretKey] != this.secret)
            {
                context.Response.StatusCode = 401;
            }
            else
            {
                await this.next(context);
            }
        }
    }
}
