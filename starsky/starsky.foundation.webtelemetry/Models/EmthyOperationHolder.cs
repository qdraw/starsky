using Microsoft.ApplicationInsights.Extensibility;

namespace starsky.foundation.webtelemetry.Models
{
	public class EmptyOperationHolder<T> : IOperationHolder<T> where T : new()
	{
		public EmptyOperationHolder()
		{
			Telemetry = new T();
		}
		
		public void Dispose()
		{
		}

		public T Telemetry { get; }
	}
}
