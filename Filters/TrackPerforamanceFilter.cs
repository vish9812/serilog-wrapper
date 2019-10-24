using Logging.Contracts;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Diagnostics;
using System.Linq;

namespace Logging.Filters
{
    public class TrackPerformanceFilter : IActionFilter
    {
        private readonly ILogger logger;

        private Stopwatch stopwatch;

        public TrackPerformanceFilter(ILogger logger)
        {
            this.logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var skipExecution = context.ActionDescriptor.FilterDescriptors.Select(fd => fd.Filter).OfType<SkipTrackPerformanceAttribute>().Any();

            if (skipExecution)
            {
                return;
            }

            stopwatch.Stop();

            logger.LogPerformance($"Executing : Controller - {Convert.ToString(context.RouteData.Values["Controller"])}, Action - {Convert.ToString(context.RouteData.Values["Action"])}", stopwatch.ElapsedMilliseconds);
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var skipExecution = context.ActionDescriptor.FilterDescriptors.Select(fd => fd.Filter).OfType<SkipTrackPerformanceAttribute>().Any();

            if (skipExecution)
            {
                return;
            }
            
            stopwatch = Stopwatch.StartNew();
        }
    }
}
