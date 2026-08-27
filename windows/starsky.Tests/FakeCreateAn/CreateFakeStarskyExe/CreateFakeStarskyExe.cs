using System.Reflection;

namespace starsky.Tests.FakeCreateAn.CreateFakeStarskyExe;

public class CreateFakeStarskyExe
{
	public CreateFakeStarskyExe()
	{
		var dirName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		if (string.IsNullOrEmpty(dirName))
		{
			return;
		}

		var folder = Path.Combine(dirName, "FakeCreateAn", "CreateFakeStarskyExe");
		WindowsExePath = Path.Combine(folder, "starsky.exe");
		UnixExePath = Path.Combine(folder, "starsky");

		if (!File.Exists(WindowsExePath))
		{
			throw new FileNotFoundException("Missing starsky.exe in " + folder);
		}

		if (!File.Exists(UnixExePath))
		{
			throw new FileNotFoundException("Missing starsky (unix) in " + folder);
		}
	}

	public string WindowsExePath { get; } = string.Empty;
	public string UnixExePath { get; } = string.Empty;

	public string ExePath =>
		OperatingSystem.IsWindows() ? WindowsExePath : UnixExePath;
}
