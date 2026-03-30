using System.Threading.Tasks;
using starsky.foundation.import.Models;

namespace starsky.foundation.import.Interfaces;

public interface ICameraMountWatcherService
{
	MountWatcherStatusModel GetStatus();
	Task<MountWatcherStatusModel> StartAsync();
	Task<MountWatcherStatusModel> StopAsync();
}

