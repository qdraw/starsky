using System;
using starsky.foundation.import.Models;

namespace starsky.foundation.import.Interfaces;

public interface IMountEventSource : IDisposable
{
	bool IsRunning { get; }
	event Action<MountAppearedEventModel>? MountAppeared;
	bool Start();
	void Stop();
}

