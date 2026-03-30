using System.Threading;
using System.Threading.Tasks;
using starsky.foundation.import.Models;

namespace starsky.foundation.import.Interfaces;

public interface IImporterCliRunner
{
	Task<ImporterCliRunResult> RunCameraImportAsync(CancellationToken cancellationToken = default);
}

