using System.Threading.Tasks;

namespace starsky.feature.syncbackground.Interfaces;

public interface IOnStartupSync
{
	Task StartUpSyncTask();
}
