using Logging.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Data;

namespace Logging
{
    public class PerformanceLogger : SeriLogger
    {
        private readonly IHttpContextAccessor contextAccessor;

        protected override string TableName => configuration[$"{configTable}:Performance"];

        public PerformanceLogger(IConfiguration configuration, IHttpContextAccessor contextAccessor)
            : base(configuration)
        {
            this.contextAccessor = contextAccessor;
        }

        public void Log(string message, long? elapsedMilliseconds, Dictionary<string, object> additionalInfo)
        {
            if (!Convert.ToBoolean(configuration[$"{configModuleStatus}:Performance"]))
            {
                return;
            }

            var logDetail = new PerformanceLogDetail();
            logDetail.PopulateModel(message, contextAccessor.HttpContext, configuration, additionalInfo, true);
            logDetail.ElapsedMilliseconds = elapsedMilliseconds;

            logger.Information("{Timestamp}{Message}{Location}{Product}" +
                    "{Hostname}" +
                    "{UserId}{UserName}{SessionId}{CorrelationId}{AdditionalInfo}{ElapsedMilliseconds}",
                    logDetail.Timestamp, logDetail.Message,
                    logDetail.Location,
                    logDetail.Product,
                    logDetail.Hostname, logDetail.UserId,
                    logDetail.UserName, logDetail.SessionId,
                    logDetail.CorrelationId,
                    logDetail.AdditionalInfoDictionary.JsonSerialize(),
                    logDetail.ElapsedMilliseconds);
        }

        protected override ColumnOptions GetColumnOptions()
        {
            var options = base.GetColumnOptions();

            options.AdditionalColumns.Add(new SqlColumn { ColumnName = nameof(PerformanceLogDetail.ElapsedMilliseconds), DataType = SqlDbType.BigInt });

            return options;
        }
    }
}
