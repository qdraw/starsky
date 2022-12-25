using System.Diagnostics;

static string  GetSolutionParentFolder()
{
	var strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
	return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(strExeFilePath))))!;
}

static Process NpmCommand(string command)
{
	var startInfo = new ProcessStartInfo()
	{
		FileName = "npm", 
		Arguments = command,
		WorkingDirectory = GetSolutionParentFolder()
	}; 
	var proc = new Process() { StartInfo = startInfo };
	return proc;
}

NpmCommand("run start").Start();

Console.WriteLine("Press any key to exit.");
Console.ReadLine();
