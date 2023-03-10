// <copyright file="CustomJsonOptions.cs" company="Stephan Santos">
// Copyright (c) Stephan Santos. All rights reserved.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;

namespace HearYe.Server.Helpers
{
    /// <summary>
    /// Preconfigured options for use with System.Text.Json.
    /// </summary>
    public static class CustomJsonOptions
    {
        /// <summary>
        /// ReferenceHandler = ReferenceHandler.IgnoreCycles. All else is default.
        /// </summary>
        /// <returns>JsonSerializerOptions.</returns>
        public static JsonSerializerOptions IgnoreCycles()
        {
            JsonSerializerOptions jsonSerializerOptions = new ()
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
            };

            return jsonSerializerOptions;
        }
    }
}
