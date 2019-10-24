using Logging.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Logging.Middlewares
{
    public class APIExceptionLoggerMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        public APIExceptionLoggerMiddleware(RequestDelegate next, ILogger logger)
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
                HandleExceptionResponse(context, exception);

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

        private static void HandleExceptionResponse(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var errorMessage = new Dictionary<string, string>();

            if (GetDebugModeFromHeader(context))
            {
                errorMessage.Add("Exception Message", exception.Message.ToString());
                if (exception.InnerException != null)
                {
                    errorMessage.Add("Inner Exception Message", exception.InnerException.ToString());
                }
                context.Response.WriteAsync(SerializeError(errorMessage));
            }
            else
            {
                context.Response.WriteAsync(SerializeError("An internal Server Error Occurred"));
            }
        }

        private static string SerializeError(object errorMessage) => (new { error = errorMessage }).JsonSerialize();

        private static bool GetDebugModeFromHeader(HttpContext context)
        {
            var isDebugEnabled = false;
            var debugVal = context.Request.Headers["IsDebugMode"];
            if (!StringValues.IsNullOrEmpty(debugVal))
            {
                bool.TryParse(debugVal.FirstOrDefault(), out isDebugEnabled);
            }

            return isDebugEnabled;
        }
    }
}
