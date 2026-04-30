using BenchmarkDotNet.Running;

namespace starsky.benchmarks.rawdng;

public static class Program
{
	public static void Main(string[] args)
	{
		BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
	}
}

