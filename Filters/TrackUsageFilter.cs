using Logging.Contracts;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace Logging.Filters
{
    public class TrackUsageFilter : IActionFilter
    {
        private readonly ILogger logger;

        public TrackUsageFilter(ILogger logger)
        {
            this.logger = logger;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var skipExecution = context.ActionDescriptor.FilterDescriptors.Select(fd => fd.Filter).OfType<SkipTrackUsageAttribute>().Any();

            if (skipExecution)
            {
                return;
            }

            logger.LogUsage($"Executing : Controller - {Convert.ToString(context.RouteData.Values["Controller"])}, Action - {Convert.ToString(context.RouteData.Values["Action"])}");
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            //noop
        }
    }
}
