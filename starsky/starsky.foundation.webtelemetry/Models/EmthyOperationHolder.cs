using System;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace starsky.foundation.webtelemetry.Models
{
	public class EmptyOperationHolder<T> : IOperationHolder<T> where T : new()
	{
		public EmptyOperationHolder()
		{
			Telemetry = new T();
		}

		// ReSharper disable once ConvertToConstant.Global
		public readonly bool Empty = true;

		public void Dispose()
		{
			// does not contain anything
		}

		public T Telemetry { get; }
	}
}
