using System.Threading.Tasks;

namespace starsky.feature.packagetelemetry.Interfaces;

public interface IPackageTelemetry
{
	Task<bool?> PackageTelemetrySend();
}
