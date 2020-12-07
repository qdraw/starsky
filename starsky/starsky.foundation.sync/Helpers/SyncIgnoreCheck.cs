using System.Linq;
using starsky.foundation.platform.Interfaces;
using starsky.foundation.platform.Models;

namespace starsky.foundation.sync.Helpers
{
	public class SyncIgnoreCheck
	{
		private readonly AppSettings _appSettings;
		private readonly IConsole _console;

		public SyncIgnoreCheck(AppSettings appSettings, IConsole console)
		{
			_appSettings = appSettings;
			_console = console;
		}
		
		public bool Filter(string subPath)
		{
			var isSynced = _appSettings.SyncIgnore.Any(subPath.StartsWith);
			if ( isSynced && _appSettings.Verbose) _console.WriteLine($"sync ignored for: {subPath}");
			return isSynced;
		}
	}
}
