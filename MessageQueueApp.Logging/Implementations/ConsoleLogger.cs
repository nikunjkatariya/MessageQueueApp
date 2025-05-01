using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageQueueApp.Logging.Enums;
using MessageQueueApp.Logging.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MessageQueueApp.Logging.Implementations
{
    public sealed class ConsoleLogger : ILogger
    {
        #region Declaration
        private static readonly Lazy<ConsoleLogger> _instance = new Lazy<ConsoleLogger>(() => new ConsoleLogger());
        private static readonly object _lock = new object();
        private string _projectName;

        // Public accessor for the singleton instance
        public static ConsoleLogger Instance => _instance.Value;
        #endregion

        // Private constructor to prevent external instantiation
        private ConsoleLogger()
        {
            LoadProjectName();
        }

        public void Log(string message, LogLevel level)
        {
            lock (_lock)
            {
                Console.ForegroundColor = level switch
                {
                    LogLevel.Info => ConsoleColor.Green,
                    LogLevel.Warning => ConsoleColor.Yellow,
                    LogLevel.Error => ConsoleColor.Red,
                    _ => ConsoleColor.White,
                };

                Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] [{_projectName ?? "UnknownProject"}] - {message}");
                Console.ResetColor();
            }
        }

        #region Private Methods
        private void LogError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [Error] - {errorMessage}");
            Console.ResetColor();
        }

        private void LoadProjectName()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                _projectName = configuration["ProjectName"];

                if (string.IsNullOrEmpty(_projectName))
                {
                    LogError("Project Name is not provided.");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error loading project name: {ex.Message}");
            }
        }
        #endregion
    }
}
