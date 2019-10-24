using System;
using System.Collections.Generic;

namespace Logging
{
    public class LoggerWrapper : Contracts.ILogger
    {
        private readonly UsageLogger usageLogger;
        private readonly ErrorLogger errorLogger;
        private readonly InfoLogger infoLogger;
        private readonly DiagnosticLogger diagnosticLogger;
        private readonly PerformanceLogger performanceLogger;

        public LoggerWrapper(
            UsageLogger usageLogger,
            ErrorLogger errorLogger,
            InfoLogger infoLogger,
            DiagnosticLogger diagnosticLogger,
            PerformanceLogger performanceLogger)
        {
            this.usageLogger = usageLogger;
            this.errorLogger = errorLogger;
            this.infoLogger = infoLogger;
            this.diagnosticLogger = diagnosticLogger;
            this.performanceLogger = performanceLogger;
        }

        public void LogError(Exception exception, Dictionary<string, object> additionalInfo = null)
        {
            errorLogger.Log(exception, additionalInfo);
        }

        public void LogUsage(string activityName, Dictionary<string, object> additionalInfo = null)
        {
            usageLogger.Log(activityName, additionalInfo);
        }

        public void LogInfo(string message, Dictionary<string, object> additionalInfo = null)
        {
            infoLogger.Log(message, additionalInfo);
        }

        public void LogDiagnostic(string message, Dictionary<string, object> additionalInfo = null)
        {
            diagnosticLogger.Log(message, additionalInfo);
        }

        public void LogPerformance(string message, long? elapsedMilliseconds, Dictionary<string, object> additionalInfo = null)
        {
            performanceLogger.Log(message, elapsedMilliseconds, additionalInfo);
        }
    }
}
