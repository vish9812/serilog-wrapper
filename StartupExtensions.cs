using Logging.Contracts;
using Logging.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Logging
{
    public static class StartupExtensions
    {
        public static void RegisterLogger(this IServiceCollection services)
        {
            services.AddSingleton<ILogger, LoggerWrapper>();

            services.AddSingleton<UsageLogger>();
            services.AddSingleton<InfoLogger>();
            services.AddSingleton<ErrorLogger>();
            services.AddSingleton<PerformanceLogger>();
            services.AddSingleton<DiagnosticLogger>();
        }

        public static IApplicationBuilder UseMvcExceptionLogger(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MvcExceptionLoggerMiddleware>();
        }

        public static IApplicationBuilder UseAPIExceptionLogger(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<APIExceptionLoggerMiddleware>();
        }
    }
}
