using Logging.Contracts;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Logging.Middlewares
{
    public class MvcExceptionLoggerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public MvcExceptionLoggerMiddleware(RequestDelegate next, ILogger logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                var exceptionType = exception.GetBaseExceptionType();

                // if exception is of type TypeLoadException then we dont want to log it condition is
                // added to handle exception thrown by Hangfire if assembly is not loaded properly
                if (exceptionType == typeof(TypeLoadException))
                {
                    throw;
                }

                logger.LogError(exception);

                throw;
            }
        }
    }
}
