using System.Threading.Tasks;
using starsky.foundation.database.Models;

namespace starsky.foundation.database.Diagnostics.Interfaces;

public interface IDiagnosticsService
{
	Task<DiagnosticsItem?> GetItem(DiagnosticsType key);
	Task<DiagnosticsItem?> AddOrUpdateItem(DiagnosticsType key, string value);
}
