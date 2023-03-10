// <copyright file="DevFileLoggerProvider.cs" company="Stephan Santos">
// Copyright (c) Stephan Santos. All rights reserved.
// </copyright>

namespace HearYe.Server
{
    /// <summary>
    /// DevFileLoggerProvider.
    /// </summary>
    public class DevFileLoggerProvider : ILoggerProvider
    {
        private string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevFileLoggerProvider"/> class.
        /// </summary>
        /// <param name="path">Disk location for log files.</param>
        public DevFileLoggerProvider(string path)
        {
            this.path = path;
        }

        /// <inheritdoc/>
        public ILogger CreateLogger(string categoryName)
        {
            return new DevFileLogger(this.path);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
        }
    }
}
