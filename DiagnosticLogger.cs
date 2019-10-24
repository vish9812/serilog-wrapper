using Logging.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Logging
{
    public class DiagnosticLogger : SeriLogger
    {
        private readonly IHttpContextAccessor contextAccessor;

        protected override string TableName => configuration[$"{configTable}:Diagnostic"];

        public DiagnosticLogger(IConfiguration configuration, IHttpContextAccessor contextAccessor)
            : base(configuration)
        {
            this.contextAccessor = contextAccessor;
        }

        public void Log(string message, Dictionary<string, object> additionalInfo)
        {
            if (!Convert.ToBoolean(configuration[$"{configModuleStatus}:Diagnostic"]))
            {
                return;
            }

            var logDetail = new DiagnosticLogDetail();
            logDetail.PopulateModel(message, contextAccessor.HttpContext, configuration, additionalInfo, false);

            logger.Information("{Timestamp}{Message}{Location}{Product}" +
                    "{Hostname}" +
                    "{UserId}{UserName}{SessionId}{CorrelationId}{AdditionalInfo}",
                    logDetail.Timestamp, logDetail.Message,
                    logDetail.Location,
                    logDetail.Product,
                    logDetail.Hostname, logDetail.UserId,
                    logDetail.UserName, logDetail.SessionId,
                    logDetail.CorrelationId,
                    logDetail.AdditionalInfoDictionary.JsonSerialize());
        }
    }
}
