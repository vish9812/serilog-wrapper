using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Logging.Filters
{
    public sealed class SkipTrackUsageAttribute : Attribute, IFilterMetadata
    {
    }
}
