#nullable enable
using System;
using System.Text.Json;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using starsky.foundation.webtelemetry.Models;

namespace starsky.foundation.webtelemetry.Helpers
{
	public static class RequestTelemetryHelper
	{
		public static string GetOperationId(this HttpContext? httpContext)
		{
			if ( httpContext == null ) return string.Empty;
			var requestTelemetry = httpContext.Features.Get<RequestTelemetry>();
			return requestTelemetry == null ? string.Empty : requestTelemetry.Context.Operation.Id;
		}

		public static IOperationHolder<DependencyTelemetry> GetOperationHolder(IServiceScopeFactory scopeFactory, string jobName, string operationId)
		{
			if ( string.IsNullOrEmpty(operationId) ) return new EmptyOperationHolder<DependencyTelemetry>();
			
			var telemetryClient = scopeFactory.CreateScope()
				.ServiceProvider.GetService<TelemetryClient>();
			if ( telemetryClient == null ) return new EmptyOperationHolder<DependencyTelemetry>();
			
			var operationHolder = telemetryClient.StartOperation<DependencyTelemetry>(
					jobName, operationId);
			operationHolder.Telemetry.Timestamp = DateTimeOffset.UtcNow;
			return operationHolder;
		}

		public static void SetData(this IOperationHolder<DependencyTelemetry>? operationHolder, object? data)
		{
			if ( data == null || operationHolder == null ) return;
			operationHolder.Telemetry.Data = JsonSerializer.Serialize(data);
			operationHolder.Telemetry.Target = "BackgroundTask";
			operationHolder.Telemetry.Type = "Task";
			operationHolder.Telemetry.ResultCode = "OK";
			operationHolder.Telemetry.Duration = DateTimeOffset.UtcNow - operationHolder.Telemetry.Timestamp;
			operationHolder.Dispose();
		}
	}
}
