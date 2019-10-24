using Logging.Models;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;

namespace Logging
{
    public abstract class SeriLogger : IDisposable
    {
        private readonly string connectionString;

        private const string configSql = "Serilog:Sql";

        protected const string configTable = configSql + ":TableName";
        protected const string configModuleStatus = "Serilog:Modules";

        protected readonly ILogger logger;
        protected readonly IConfiguration configuration;

        protected abstract string TableName { get; }

        public SeriLogger(IConfiguration configuration)
        {
            this.configuration = configuration;

            connectionString = configuration.GetConnectionString("LoggingConnectionString");

            logger = new LoggerConfiguration()
                .WriteTo.MSSqlServer(
                connectionString: connectionString,
                schemaName: configuration[$"{configSql}:SchemaName"],
                tableName: TableName,
                autoCreateSqlTable: Convert.ToBoolean(configuration[$"{configSql}:AutoCreateSqlTable"]),
                columnOptions: GetColumnOptions(),
                restrictedToMinimumLevel: (LogEventLevel)Convert.ToInt32(configuration[$"{configSql}:RestrictedToMinimumLevel"]),
                batchPostingLimit: Convert.ToInt32(configuration[$"{configSql}:BatchPostingLimit"]),
                period: TimeSpan.Parse(configuration[$"{configSql}:Period"]))
                .CreateLogger();

            if (Convert.ToBoolean(configuration[$"Serilog:SelfLog"]))
            {
                SelfLog.Enable(message =>
                {
                    var directoryPath = $@"Serilog\{TableName}";
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    File.AppendAllText($"{directoryPath}\\SerilogDebug_{DateTime.Now.ToFileTimeUtc()}.txt", message);
                });
            }
        }

        protected virtual ColumnOptions GetColumnOptions()
        {
            var options = new ColumnOptions();
            options.DisableTriggers = true;
            options.Store.Clear();

            options.AdditionalColumns = new Collection<SqlColumn>
            {
                new SqlColumn {ColumnName = nameof(LogDetail.Timestamp), DataType = SqlDbType.DateTime, AllowNull = false},
                new SqlColumn {ColumnName = nameof(LogDetail.Product), DataType = SqlDbType.NVarChar, DataLength = 100},
                new SqlColumn {ColumnName = nameof(LogDetail.Message), DataType = SqlDbType.NVarChar},
                new SqlColumn {ColumnName = nameof(LogDetail.UserId), DataType = SqlDbType.Int},
                new SqlColumn {ColumnName = nameof(LogDetail.UserName), DataType = SqlDbType.NVarChar, DataLength = 200},
                new SqlColumn {ColumnName = nameof(LogDetail.Location), DataType = SqlDbType.NVarChar, DataLength = 500},
                new SqlColumn {ColumnName = nameof(LogDetail.Hostname), DataType = SqlDbType.NVarChar, DataLength = 100},
                new SqlColumn {ColumnName = nameof(LogDetail.SessionId), DataType = SqlDbType.VarChar, DataLength = 100},
                new SqlColumn {ColumnName = nameof(LogDetail.CorrelationId), DataType = SqlDbType.VarChar, DataLength = 100},
                new SqlColumn {ColumnName = nameof(LogDetail.AdditionalInfo), DataType = SqlDbType.NVarChar},
            };

            return options;
        }

        public void Dispose()
        {
            (logger as Logger)?.Dispose();
        }
    }
}
