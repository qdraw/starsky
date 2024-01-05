using System.Threading.Tasks;
using OpenTelemetry.Proto.Trace.V1;

namespace starsky.foundation.webtelemetry.Interfaces;

public interface ITracesService
{
	Task SendTrace(TracesData tracesData);
}
