namespace starsky.foundation.worker.CpuEventListener.Interfaces;

public interface ICpuUsageListener
{
	/// <summary>
	/// Last CPU usage
	/// </summary>
	double CpuUsageMean { get; }
}
