using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.IO;
using System.Threading;

namespace DeviceLogLoader
{
    public static class Logger
    {
        private class ThreadIdEnricher : ILogEventEnricher
        {
            public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadId", Thread.CurrentThread.ManagedThreadId));
            }
        }
        
        private const string _logFileNameWithoutExtension = "Log-Loader-";
        private const string _logLineTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss,fff}] [{Level:u3}] [{ThreadId}] {Message:lj}{NewLine}{Exception}";
        private static readonly string _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"logs/{_logFileNameWithoutExtension}.log");
        
        public static void Configure()
        {
            Log.Logger = new LoggerConfiguration()
                .Enrich.With(new ThreadIdEnricher())
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif
                .WriteTo.File(path: _logFilePath, rollingInterval: RollingInterval.Day, outputTemplate: _logLineTemplate)
                .CreateLogger();
        }
    }
}