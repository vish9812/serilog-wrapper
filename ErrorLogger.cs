using Logging.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.Generic;
using System.Data;

namespace Logging
{
    public class ErrorLogger : SeriLogger
    {
        private readonly IHttpContextAccessor contextAccessor;

        protected override string TableName => configuration[$"{configTable}:Error"];

        public ErrorLogger(IConfiguration configuration, IHttpContextAccessor contextAccessor)
            : base(configuration)
        {
            this.contextAccessor = contextAccessor;
        }

        public void Log(Exception exception, Dictionary<string, object> additionalInfo)
        {
            if (!Convert.ToBoolean(configuration[$"{configModuleStatus}:Error"]))
            {
                return;
            }

            var logDetail = new ErrorLogDetail();
            logDetail.PopulateModel(exception?.Message, contextAccessor.HttpContext, configuration, additionalInfo, false);
            logDetail.Exception = exception?.ToBetterString();
            logDetail.ExceptionBaseType = exception?.GetBaseExceptionType().FullName;
            logDetail.Level = LogLevel.Error.ToString();

            logger.Information("{Timestamp}{Message}{Location}{Product}" +
                    "{Hostname}" +
                    "{UserId}{UserName}{SessionId}{CorrelationId}{AdditionalInfo}{ExceptionBaseType}{Exception}{Level}",
                    logDetail.Timestamp, logDetail.Message,
                    logDetail.Location,
                    logDetail.Product,
                    logDetail.Hostname, logDetail.UserId,
                    logDetail.UserName, logDetail.SessionId,
                    logDetail.CorrelationId,
                    logDetail.AdditionalInfoDictionary.JsonSerialize(),
                    logDetail.ExceptionBaseType,
                    logDetail.Exception,
                    logDetail.Level);
        }

        protected override ColumnOptions GetColumnOptions()
        {
            var options = base.GetColumnOptions();

            options.AdditionalColumns.Add(new SqlColumn { ColumnName = nameof(ErrorLogDetail.ExceptionBaseType), DataType = SqlDbType.NVarChar, DataLength = 500 });
            options.AdditionalColumns.Add(new SqlColumn { ColumnName = nameof(ErrorLogDetail.Exception), DataType = SqlDbType.NVarChar });
            options.AdditionalColumns.Add(new SqlColumn { ColumnName = nameof(ErrorLogDetail.Level), DataType = SqlDbType.VarChar, DataLength = 100 });

            return options;
        }
    }
}
