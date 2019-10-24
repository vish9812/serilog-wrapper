using System;
using System.Collections.Generic;

namespace Logging.Contracts
{
    public interface ILogger
    {
        void LogUsage(string activityName, Dictionary<string, object> additionalInfo = null);

        void LogError(Exception exception, Dictionary<string, object> additionalInfo = null);

        void LogInfo(string message, Dictionary<string, object> additionalInfo = null);

        void LogDiagnostic(string message, Dictionary<string, object> additionalInfo = null);

        void LogPerformance(string message, long? elapsedMilliseconds, Dictionary<string, object> additionalInfo = null);
    }
}
