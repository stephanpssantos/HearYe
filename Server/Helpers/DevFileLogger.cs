// <copyright file="DevFileLogger.cs" company="Stephan Santos">
// Copyright (c) Stephan Santos. All rights reserved.
// </copyright>

namespace HearYe.Server
{
    /// <summary>
    /// Provides the option to log to disk. Only for use in development.
    /// In production, use a third party logger or the logger provided
    /// by the hosting service.
    /// </summary>
    public class DevFileLogger : ILogger
    {
        private static readonly object Lock = new object();
        private string filePath = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="DevFileLogger"/> class.
        /// </summary>
        /// <param name="path">Disk location for log files.</param>
        public DevFileLogger(string path)
        {
            this.filePath = path;
        }

        /// <inheritdoc/>
#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
        public IDisposable BeginScope<TState>(TState state)
#pragma warning restore CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
        {
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (formatter != null)
            {
                lock (Lock)
                {
                    if (!System.IO.Directory.Exists(this.filePath))
                    {
                        System.IO.Directory.CreateDirectory(this.filePath);
                    }

                    string fullFilePath = Path.Combine(this.filePath, DateTime.Now.ToString("yyyy-MM-dd") + "_log.txt");
                    var n = Environment.NewLine;
                    string exc = string.Empty;
                    if (exception != null)
                    {
                        exc = n + exception.GetType() + ": " + exception.Message + n + exception.StackTrace + n;
                    }

                    File.AppendAllText(
                        fullFilePath,
                        logLevel.ToString() + ": " + DateTime.Now.ToString() + " " + formatter(state, exception) + n + exc);
                }
            }
        }
    }
}
