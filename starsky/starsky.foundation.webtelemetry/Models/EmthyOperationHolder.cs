using System;
using Microsoft.ApplicationInsights.Extensibility;

namespace starsky.foundation.webtelemetry.Models
{
	public sealed class EmptyOperationHolder<T> : IOperationHolder<T> where T : new()
	{
		public EmptyOperationHolder()
		{
			Telemetry = new T();
		}

		// ReSharper disable once ConvertToConstant.Global
		public readonly bool Empty = true;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private bool IsDisposed { get; set; }

		private void Dispose(bool disposing)
		{
			if ( disposing && !IsDisposed )
			{
				IsDisposed = true;
			}
		}

		public T Telemetry { get; }
	}
}
