// <copyright file="SecretHeaderMiddlewareExtensions.cs" company="Stephan Santos">
// Copyright (c) Stephan Santos. All rights reserved.
// </copyright>

namespace HearYe.Server.Helpers
{
    /// <summary>
    /// Extension class. Use by calling app.UseSecretHeader().
    /// </summary>
    public static class SecretHeaderMiddlewareExtensions
    {
        /// <summary>
        /// Add middleware to IApplicationBuilder.
        /// </summary>
        /// <param name="builder">IApplicationBuilder.</param>
        /// <param name="secret">Expected secret value.</param>
        /// <param name="secretKey">Header key containing secret.</param>
        /// <returns>Builder with this middleware.</returns>
        public static IApplicationBuilder UseSecretHeader(
            this IApplicationBuilder builder, string secret, string secretKey)
        {
            return builder.UseMiddleware<SecretHeaderMiddleware>(secret, secretKey);
        }
    }
}
