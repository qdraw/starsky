using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace starsky.foundation.databasetelemetry.Services
{
	/// <summary>
	/// @see: https://amuratgencay.medium.com/how-to-track-postgresql-queries-using-entityframework-core-in-application-insights-2f173d6c636d
	/// </summary>
    public class DatabaseTelemetryInterceptor : IDbCommandInterceptor
    {
        private readonly TelemetryClient _telemetryClient;

        private const string TelemetryType = "Database";
        
        public DatabaseTelemetryInterceptor(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        private static string GetSqlName(DbCommand command)
        {
	        var name = "SQLDatabase";
	        if (command.Connection != null)
	        {
		        name = $"{command.Connection.DataSource} | {command.Connection.Database}";
	        }
	        return name;
        }
        
        private void TrackDependency(DbCommand command, DateTimeOffset startTime, bool success = true)
        {
	        var duration = TimeSpan.Zero;
	        if (startTime != default(DateTimeOffset))
	        {
		        duration = DateTimeOffset.UtcNow - startTime;
	        }

	        var commandName = command.CommandText;
	        _telemetryClient.TrackDependency(new DependencyTelemetry()
	        {
		        Name = GetSqlName(command),
		        Data = commandName,
		        Type = TelemetryType,
		        Duration = duration,
		        Timestamp = startTime,
		        Success = success
	        });
        }

        public InterceptionResult<DbCommand> CommandCreating(CommandCorrelatedEventData eventData,
	        InterceptionResult<DbCommand> result)
        {
	        return result;
        }

        public DbCommand CommandCreated(CommandEndEventData eventData, DbCommand result)
        {
	        return result;
        }

        public InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command,
	        CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
	        return result;
        }

        public InterceptionResult<object> ScalarExecuting(DbCommand command,
	        CommandEventData eventData, InterceptionResult<object> result)
        {
	        return result;
        }

        public InterceptionResult<int> NonQueryExecuting(DbCommand command,
	        CommandEventData eventData, InterceptionResult<int> result)
        {
	        return result;
        }

        public Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData,
	        InterceptionResult<DbDataReader> result,
	        CancellationToken cancellationToken = new CancellationToken())
        {
	        return Task.FromResult(result);
        }

        public Task<InterceptionResult<object>> ScalarExecutingAsync(DbCommand command, CommandEventData eventData,
	        InterceptionResult<object> result,
	        CancellationToken cancellationToken = new CancellationToken())
        {
	        return Task.FromResult(result);
        }

        public Task<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData,
	        InterceptionResult<int> result,
	        CancellationToken cancellationToken = new CancellationToken())
        {
	        return Task.FromResult(result);
        }

        public DbDataReader ReaderExecuted(DbCommand command,
	        CommandExecutedEventData eventData, DbDataReader result)
        {
	        return result;
        }

        public object ScalarExecuted(DbCommand command, CommandExecutedEventData eventData,
	        object result)
        {
	        return result;
        }

        public int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData,
	        int result)
        {
	        return result;
        }

        public Task<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
	        DbDataReader result,
	        CancellationToken cancellationToken = new CancellationToken())
        {
	        return Task.FromResult(result);
        }

        public Task<object> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData,
	        object result,
	        CancellationToken cancellationToken = new CancellationToken())
        {
	        return Task.FromResult(result);
        }

        public Task<int> NonQueryExecutedAsync(DbCommand command,
	        CommandExecutedEventData eventData, int result,
	        CancellationToken cancellationToken = new CancellationToken())
        {
	        return Task.FromResult(result);
        }

        public void CommandFailed(DbCommand command, CommandErrorEventData eventData)
        {
        }

        public Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData,
	        CancellationToken cancellationToken = new CancellationToken())
        {
	        return Task.CompletedTask;
        }

        public InterceptionResult DataReaderDisposing(DbCommand command,
	        DataReaderDisposingEventData eventData, InterceptionResult result)
        {
	        if ( command.CommandText.Contains("__EFMigrationsHistory") || command.CommandText.Contains("SearchSuggestionsService"))
	        {
		        return result;
	        }
	        
	        TrackDependency(command, eventData.StartTime);
	        
	        return result;
        }
    }
}
